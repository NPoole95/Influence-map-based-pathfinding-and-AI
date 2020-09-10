using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class DefenderController : MovingObject
{
    private float attackRange = 0.5f;
    private float attackDamage = 20.0f; // the amount of damage dealt per attack
    private float attackSpeed = 1.0f; // the interval in seconds between attacks
    private const int MAXHP = 80;
    private float attackTimer = 0.0f;

    public AStar AStarRef;
    public InfluenceMapController imController;
    public BarracksController barracksController;
    public LoadMapFromFile LMFF;
    public List<AStar.mapNode> path;
    int currentPathNode = 0;

    private Transform camera; // used for the camera lookat 
    public GameObject healthBar;
    public Slider healthBarSlider;


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
            agent.GetComponent<Animator>().SetBool("Dieing", true);
            --barracksController.numberOfDefenders;
            transform.gameObject.tag = "Untagged";
            Destroy(gameObject);
        }
        if (currentFighterState == fighterState.searching)
        {
            agent.GetComponent<Animator>().SetBool("Moving", false);
            agent.GetComponent<Animator>().SetBool("Idle", true);
            agent.GetComponent<Animator>().SetBool("Attacking", false);
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
            agent.GetComponent<Animator>().SetBool("Moving", true);
            agent.GetComponent<Animator>().SetBool("Idle", false);
            agent.GetComponent<Animator>().SetBool("Attacking", false);
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
            agent.GetComponent<Animator>().SetBool("Idle", false);
            agent.GetComponent<Animator>().SetBool("Moving", false);
            agent.GetComponent<Animator>().SetBool("Attacking", true);


            currentTarget = FindClosestEnemy("Fighter - Ground");

            if (currentTarget == null)
            {
                currentFighterState = fighterState.searching;
            }
            else
            {
                attackTimer -= Time.deltaTime;

              if (attackTimer <= 0.0f)
              {
                  attackTimer = attackSpeed;
                  currentTarget.GetComponent<MovingObject>().HP -= attackDamage;
              }
              
              if (currentTarget.GetComponent<MovingObject>().HP <= 0.0f)
              {
                  path.Clear();
                  currentFighterState = fighterState.searching;
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
        gos = GameObject.FindGameObjectsWithTag("Defence"); // finds game objects with the defence tag and loads them into an array
        GameObject attackPoint = null; // creates a game object to store the best attack point
        GameObject testPoint; // another game object to hold the object currently being tested

        float IMValue = 1000;
        float CurrentIMValue;
        Vector3 position;
        List<AStar.mapNode> currentPath = new List<AStar.mapNode>();

        foreach (GameObject go in gos) // checks each game object "go" in th game object array
        {
            testPoint = go;
            position = go.transform.position; //sets position to the position of the game object searching (the knight)
            CurrentIMValue = imController.influenceMap[(int)position.x, (int)position.z];

            if (CurrentIMValue < IMValue)
            {
                IMValue = CurrentIMValue;
                attackPoint = testPoint;
            }
        }

        gos = GameObject.FindGameObjectsWithTag("Wall");

        foreach (GameObject go in gos) // checks each game object "go" in th game object array
        {
            testPoint = go;
            position = go.transform.position; //sets position to the position of the game object searching (the knight)
            CurrentIMValue = imController.influenceMap[(int)position.x, (int)position.z];

            if (CurrentIMValue < IMValue)
            {
                IMValue = CurrentIMValue;
                attackPoint = testPoint;
            }
        }

        currentPath = AStarRef.AStarSearch((int)transform.position.x, (int)transform.position.z, (int)attackPoint.transform.position.x, (int)attackPoint.transform.position.z);

        if (currentPath == null || currentPath.Count == 0)
        {
            return null;
        }
        else
        {
            path = currentPath.ConvertAll(x => new AStar.mapNode(x));
        }
        return attackPoint;
    }

    //////////////////////////// Alternate 'find' methods that use pathfinding to ifnd the nearest point to attack

   protected new GameObject FindClosestEnemy(string tag)
   {
       GameObject[] gos; // an array holding the game objects of all enemies
       gos = GameObject.FindGameObjectsWithTag(tag); // finds game objects with the defence tag and loads them into an array

       GameObject closest = null; // creates another game object to hold the closest one

       Vector3 position = transform.position; //sets position to the position of the game object searching (the knight)

       foreach (GameObject go in gos) // checks each game object "go" in th game object array
       {
            float distance = (Mathf.Abs(go.transform.position.x) - Mathf.Abs(position.x)) + (Mathf.Abs(go.transform.position.z) - Mathf.Abs(position.z)); // gets the distance between the found object and the object doing the search

            if (distance > 5.0f)
            {
                continue;
            }
            else
            {
                closest = go;
            }           
       }
       return closest; // returns the closest enemy within its radius
   }
}
