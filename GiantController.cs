using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GiantController : MovingObject
{
    private float attackRange = 0.8f;
    private float attackDamage = 30.0f; // the amount of damage dealt per attack
    private float attackSpeed = 1.5f; // the interval in seconds between attacks
    private const int MAXHP = 120;

    private float attackTimer = 0.0f;

    public AStar AStarRef;
    public InfluenceMapController imController;
    public LoadMapFromFile LMFF;
    public List<AStar.mapNode> path;
    int currentPathNode = 0;

    private Transform camera; // used for the camera lookat 
    public GameObject healthBar;
    public Slider healthBarSlider;
    public AudioSource audioSource;

    ////////////Variables used when searching//////////////
    GameObject[] gos; // an array holding the game objects of all enemies
    GameObject attackPoint = null; // creates a game object to store the best attack point
    GameObject testPoint; // another game object to hold the object currently being tested
    float LowestIMValue;
    float CurrentIMValue;
    Vector3 position;
    List<AStar.mapNode> currentPath;
    List<AttackPoint> attackPoints;

    struct AttackPoint
    {
        public GameObject GO;
        public int pathLength;
        public List<AStar.mapNode> Path;
        public AttackPoint(GameObject go, List<AStar.mapNode> path)
        {
            Path = path;
            pathLength = 0;
            if (path != null)
            {
                pathLength = path.Count;
            }
            GO = go;
        }
    };
    // Start is called before the first frame update
    public override void Awake()
    {
        AStarRef = GameObject.Find("AStar").GetComponent<AStar>();
        LMFF = GameObject.Find("LoadMapFromFile").GetComponent<LoadMapFromFile>();
        imController = GameObject.Find("InfluenceMapController").GetComponent<InfluenceMapController>();
        path = new List<AStar.mapNode>();
        HP = MAXHP;
        camera = Camera.main.transform;
        healthBarSlider.value = CalculateHealth();
        base.Awake();
    }

    // Update is called once per frame
    public override void Update()
    {
        healthBarSlider.value = CalculateHealth();
        healthBarSlider.transform.LookAt(camera);

        if (HP <= 0)
        {
            currentFighterState = fighterState.dead;
        }
        else if (HP < MAXHP)
        {
            healthBar.SetActive(true);
        }
        if (currentFighterState == fighterState.dead)
        {
            transform.gameObject.tag = "Untagged";
            Destroy(gameObject);
        }
        if (currentFighterState == fighterState.searching)
        {
            currentPathNode = 0;
            currentTarget = findBestAttackPoint("Defence");

            if (currentTarget == null)
            {
                //currentTarget = FindClosestWall();
                currentTarget = findBestAttackPoint("Wall");
            }
            if (currentTarget != null)
            {
                startMovement();
            }
        }
        if (currentFighterState == fighterState.moving)
        {
            agent.isStopped = false;

            if (currentTarget == null)
            {
                currentFighterState = fighterState.searching;
            }
            if (!agent.pathPending)
            {
                if (agent.remainingDistance <= attackRange)
                {
                    if (currentPathNode < path.Count) // if the agent has not yet completed their path, continue to the next node
                    {
                        startMovement();
                        currentPathNode++;
                    }
                    else
                    {
                        Debug.Log("Attacking");
                        currentPathNode = 0;
                        agent.isStopped = true;
                        currentFighterState = fighterState.attacking;
                    }
                }
            }
        }
        if (currentFighterState == fighterState.attacking)
        {
            agent.GetComponent<Animator>().SetBool("Idle", true);
            agent.GetComponent<Animator>().SetBool("Moving", false);

            attackTimer -= Time.deltaTime;

            if (attackTimer <= 0.0f)
            {
                attackTimer = attackSpeed;
                currentTarget.GetComponent<StationaryObject>().HP -= attackDamage;
                audioSource.Play();
            }
            if (currentTarget.GetComponent<StationaryObject>().HP <= 0.0f)
            {
                Debug.Log("its dead");
                path.Clear();
                currentFighterState = fighterState.searching;
            }
        }
    }
    float CalculateHealth()
    {
        return HP / MAXHP;
    }

    private void startMovement()
    {
        agent.SetDestination(currentTarget.transform.position); // sets the agents destination to the point that was clicked
        currentFighterState = fighterState.moving;
        if (agent.CompareTag("Fighter - Ground"))
        {
            agent.GetComponent<Animator>().SetBool("Moving", true);
            agent.GetComponent<Animator>().SetBool("Idle", false);
        }
    }

    public void enableHealthBar(Transform parent)
    {
        foreach (Transform child in parent)
        {
            if (child.CompareTag("HealthBar"))
            {
                child.gameObject.SetActive(true);
            }
        }
    }

    private GameObject findBestAttackPoint(string tag)
    {

        gos = GameObject.FindGameObjectsWithTag(tag); // finds game objects with the defence tag and loads them into an array
        attackPoint = null; // creates a game object to store the best attack point
        LowestIMValue = 1000;
        currentPath = new List<AStar.mapNode>();
        attackPoints = new List<AttackPoint>();
        ;
        if (gos.Length == 0)
        {
            return null;
        }

        foreach (GameObject go in gos) // checks each game object "go" in th game object array
        {
            testPoint = go;
            position = go.transform.position; //sets position to the position of the game object searching (the knight)
            CurrentIMValue = imController.influenceMap[(int)position.x, (int)position.z];

            if (CurrentIMValue < LowestIMValue)
            {
                currentPath = AStarRef.AStarSearch((int)transform.position.x, (int)transform.position.z, (int)testPoint.transform.position.x, (int)testPoint.transform.position.z);
                if (currentPath != null)
                {
                    attackPoints.Clear();
                    LowestIMValue = CurrentIMValue;
                    attackPoints.Add(new AttackPoint(go, currentPath));
                }
            }
            else if (CurrentIMValue == LowestIMValue)
            {
                currentPath = AStarRef.AStarSearch((int)transform.position.x, (int)transform.position.z, (int)testPoint.transform.position.x, (int)testPoint.transform.position.z);
                if (currentPath != null)
                {
                    attackPoints.Add(new AttackPoint(go, currentPath));
                }
            }
        }

        if (attackPoints.Count == 0)
        {
            attackPoints.Clear();
            return null;
        }
        else if (attackPoints.Count == 1)
        {
            attackPoint = attackPoints[0].GO;
            path = attackPoints[0].Path.ConvertAll(x => new AStar.mapNode(x));
            attackPoints.Clear();
            return attackPoint;
        }
        else
        {
            attackPoints.Sort((s1, s2) => s1.pathLength.CompareTo(s2.pathLength));
            attackPoint = attackPoints[0].GO;
            path = attackPoints[0].Path.ConvertAll(x => new AStar.mapNode(x));
            attackPoints.Clear();
            return attackPoint;
        }
    }
    //protected new GameObject findBestAttackPoint(string tag)
    //{
    //    GameObject[] gos; // an array holding the game objects of all enemies
    //    gos = GameObject.FindGameObjectsWithTag(tag); // finds game objects with the defence tag and loads them into an array
    //    GameObject attackPoint = null; // creates a game object to store the best attack point
    //    GameObject testPoint; // another game object to hold the object currently being tested

    //    float IMValue = 1000;
    //    float CurrentIMValue;
    //    Vector3 position;
    //    List<AStar.mapNode> currentPath = new List<AStar.mapNode>();

    //    foreach (GameObject go in gos) // checks each game object "go" in th game object array
    //    {
    //        testPoint = go;
    //        position = go.transform.position; //sets position to the position of the game object searching (the knight)
    //        CurrentIMValue = imController.influenceMap[(int)position.x, (int)position.z];

    //        if (CurrentIMValue < IMValue)
    //        {
    //            IMValue = CurrentIMValue;
    //            attackPoint = testPoint;
    //        }
    //    }
    //    currentPath = AStarRef.AStarSearch((int)transform.position.x, (int)transform.position.z, (int)attackPoint.transform.position.x, (int)attackPoint.transform.position.z);

    //    if (currentPath == null || currentPath.Count == 0)
    //    {
    //        return null;
    //    }
    //    else
    //    {
    //        path = currentPath.ConvertAll(x => new AStar.mapNode(x));
    //    }
    //    return attackPoint;
    //}



    //protected new GameObject FindClosestWall()
    //{
    //    GameObject[] gos;
    //    gos = GameObject.FindGameObjectsWithTag("Wall"); // finds game objects with the defence tag and loads them into an array

    //    GameObject closest = null; // creates another game object to hold the closest one

    //    Vector3 position = transform.position; //sets position to the position of the game object searching

    //    List<AStar.mapNode> currentPath = new List<AStar.mapNode>(); // holds the lengh of the current path being evaluated. starts at infinity so that the initial value is lower in the initial check

    //    foreach (GameObject go in gos) // checks each game object "go" in th game object array
    //    {
    //        currentPath = AStarRef.AStarSearch((int)position.x, (int)position.z, (int)go.transform.position.x, (int)go.transform.position.z);

    //        if (currentPath == null)
    //        {
    //            continue;
    //        }
    //        else if (path.Count == 0)
    //        {
    //            path = currentPath.ConvertAll(x => new AStar.mapNode(x));
    //            closest = go;
    //        }
    //        else if (currentPath[currentPath.Count - 1].totalCost < path[path.Count - 1].totalCost) // if current path is empty, or new path is better
    //        {
    //            path = currentPath.ConvertAll(x => new AStar.mapNode(x));
    //            closest = go;
    //        }

    //        currentPath.Clear();

    //    }
    //    return closest; // returns a game object with the shortest path
    //}
}
