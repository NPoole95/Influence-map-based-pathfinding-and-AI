using System.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class KnightController : MovingObject
{
    private float attackRange = 0.5f;
    private float attackDamage = 20.0f; // the amount of damage dealt per attack
    private float attackSpeed = 1.0f; // the interval in seconds between attacks
    private const int MAXHP = 80;
    private float attackTimer = 0.0f;

    public AStar AStarRef;
    public InfluenceMapController imController;
    public LoadMapFromFile LMFF;
    List<AStar.mapNode> path;
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
                Debug.Log("it happened?");
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
                        currentPathNode = 0;
                        agent.isStopped = true;
                        currentFighterState = fighterState.attacking;
                    }
                }
            }
        }
        if (currentTarget == null)
        {
            currentFighterState = fighterState.searching;
        }
        if (currentFighterState == fighterState.attacking)
        {

            float distance = Mathf.Abs(currentTarget.transform.position.x - transform.position.x) + Mathf.Abs(currentTarget.transform.position.z - transform.position.z);

            if (distance > 1.5f)
            {
                currentFighterState = fighterState.searching;
            }
            else
            {
                agent.GetComponent<Animator>().SetBool("Idle", true);
                agent.GetComponent<Animator>().SetBool("Moving", false);

                attackTimer -= Time.deltaTime;

                if (currentTarget.GetComponent<StationaryObject>().HP <= 0.0f)
                {
                    path.Clear();
                    currentFighterState = fighterState.searching;
                }
                else if (attackTimer <= 0.0f)
                {
                    attackTimer = attackSpeed;
                    currentTarget.GetComponent<StationaryObject>().HP -= attackDamage;
                    audioSource.Play();
                }
            }
        }
    }

    float CalculateHealth()
    {
        return HP / MAXHP;
    }

    private void startMovement()
    {
        for (int i = currentPathNode; i < path.Count - 1; i++)
        {
            Vector3 start = new Vector3(path[i].x, 0.2f, path[i].y);
            Vector3 end = new Vector3(path[i + 1].x, 0.2f, path[i + 1].y);

            Debug.DrawLine(start, end, Color.white, 2.0f, true);
        }

        agent.SetDestination(new Vector3(path[currentPathNode].x, agent.transform.position.y, path[currentPathNode].y));

        currentFighterState = fighterState.moving;
        if (agent.CompareTag("Fighter - Ground"))
        {
            agent.GetComponent<Animator>().SetBool("Moving", true);
            agent.GetComponent<Animator>().SetBool("Idle", false);
        }
    }


    // method to find the best point of attack based on the values of the influence map and using the shortest possible path
    private GameObject findBestAttackPoint(string tag)
    {

        gos = GameObject.FindGameObjectsWithTag(tag); // finds game objects with the defence tag and loads them into an array
        attackPoint = null; // creates a game object to store the best attack point
        LowestIMValue = 1000; // This is set to an extremely high value that will always be replaced by the influence value of the first defence searched
        currentPath = new List<AStar.mapNode>();
        attackPoints = new List<AttackPoint>();

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

    // method to find the best point of attack based on the values of the influence map and using the closest possible point
    private GameObject findClosestAttackPoint(string tag)
    {

        GameObject[] gos; // an array holding the game objects of all enemies
        gos = GameObject.FindGameObjectsWithTag(tag); // finds game objects with the defence tag and loads them into an array
        GameObject attackPoint = null; // creates a game object to store the best attack point
        GameObject testPoint; // another game object to hold the object currently being tested
        float lowestDistance = Mathf.Infinity;
        float IMValue = 1000;
        float CurrentIMValue;
        Vector3 position;
        List<AStar.mapNode> currentPath = new List<AStar.mapNode>();

        List<AttackPoint> potentialTargets = new List<AttackPoint>();

        foreach (GameObject go in gos) // checks each game object "go" in th game object array
        {
            testPoint = go;
            position = go.transform.position; //sets position to the position of the game object searching (the knight)
            CurrentIMValue = imController.influenceMap[(int)position.x, (int)position.z];

            if (CurrentIMValue <= IMValue)
            {
                IMValue = CurrentIMValue;
                potentialTargets.Add(new AttackPoint(go, null));
            }
        }

        foreach (AttackPoint potentialTarget in potentialTargets)
        {

            float distance = Mathf.Abs(potentialTarget.GO.transform.position.x - transform.position.x) + Mathf.Abs(potentialTarget.GO.transform.position.z - transform.position.z);

            if (distance < lowestDistance)
            {
                currentTarget = potentialTarget.GO;
                lowestDistance = distance;
            }
        }

        currentPath = AStarRef.AStarSearch((int)transform.position.x, (int)transform.position.z, (int)currentTarget.transform.position.x, (int)currentTarget.transform.position.z);

        if (currentPath == null || currentPath.Count == 0)
        {
            return null;
        }
        else
        {
            path = currentPath.ConvertAll(x => new AStar.mapNode(x));
            attackPoint = currentTarget;
        }
        return attackPoint;
    }
}