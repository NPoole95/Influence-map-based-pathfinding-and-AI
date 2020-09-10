using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class MazeKnightController : MovingObject
{
    private float attackRange = 0.5f;
    private float attackDamage = 20.0f; // the amount of damage dealt per attack
    private float attackSpeed = 1.0f; // the interval in seconds between attacks
    private const int MAXHP = 80;
    private float attackTimer = 0.0f;

    public AStarDeathMap AStarRef;
    public DeathMapController dmController;
    public LoadDeathMapFromFile LMFF;
    public List<AStarDeathMap.mapNode> path;
    int currentPathNode = 0;

    private Transform camera; // used for the camera lookat 
    public GameObject healthBar;
    public Slider healthBarSlider;


    // Start is called before the first frame update
    public override void Awake()
    {
        AStarRef = GameObject.Find("AStarDeathMap").GetComponent<AStarDeathMap>();
        LMFF = GameObject.Find("LoadDeathMapFromFile").GetComponent<LoadDeathMapFromFile>();
        dmController = GameObject.Find("DeathMapController").GetComponent<DeathMapController>();
        path = new List<AStarDeathMap.mapNode>();
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
            dmController.AddDeath((int)transform.position.x, (int)transform.position.z);
            transform.gameObject.tag = "Untagged";
            Destroy(gameObject);
        }
        if (currentFighterState == fighterState.searching)
        {
            currentPathNode = 0;
            currentTarget = findBestAttackPoint("Resource");

            if (currentTarget == null)
            {
                //currentTarget = FindClosestWall();
                Debug.LogError("NO PATH");
            }
            if (currentTarget != null)
            {
                startMovement();
            }
        }
        if (currentFighterState == fighterState.moving)
        {
            agent.isStopped = false;
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

            agent.GetComponent<Animator>().SetBool("Idle", true);
            agent.GetComponent<Animator>().SetBool("Moving", false);         
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


    // method to find the best point of attack based on the values of the influence map
    protected new GameObject findBestAttackPoint(string tag)
    {
        GameObject[] gos; // an array holding the game objects of all enemies
        gos = GameObject.FindGameObjectsWithTag(tag); // finds game objects with the defence tag and loads them into an array
        GameObject attackPoint = null; // creates a game object to store the best attack point
        GameObject testPoint; // another game object to hold the object currently being tested

        float IMValue = 1000;
        float CurrentIMValue;
        Vector3 position;
        List<AStarDeathMap.mapNode> currentPath = new List<AStarDeathMap.mapNode>();

        if (gos.Length == 0)
        {
            return null;
        }

        foreach (GameObject go in gos) // checks each game object "go" in th game object array
        {
            testPoint = go;
            position = go.transform.position; //sets position to the position of the game object searching (the knight)
            CurrentIMValue = dmController.influenceMap[(int)position.x, (int)position.z];

            if (CurrentIMValue < IMValue)
            {
                currentPath = AStarRef.AStarSearch((int)transform.position.x, (int)transform.position.z, (int)testPoint.transform.position.x, (int)testPoint.transform.position.z);

                if (currentPath != null)
                {
                    IMValue = CurrentIMValue;
                    attackPoint = testPoint;
                }
            }
        }

        if (currentPath == null || currentPath.Count == 0)
        {
            return null;
        }
        else
        {
            path = currentPath.ConvertAll(x => new AStarDeathMap.mapNode(x));
        }
        return attackPoint;
    }
}
