using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class KnightControllerNew : MovingObject
{
    private float attackRange = 0.5f;
    private float attackDamage = 20.0f; // the amount of damage dealt per attack
    private float attackSpeed = 1.0f; // the interval in seconds between attacks

    private float attackTimer = 0.0f;

    public AStar AStarRef;
    public List<AStar.mapNode> path = new List<AStar.mapNode>();
    int currentPathNode = 0;


    // Start is called before the first frame update
    public override void Awake()
    {
        AStarRef = GameObject.Find("AStar").GetComponent<AStar>();



        base.Awake();
    }

    // Update is called once per frame
    public override void Update()
    {

        if (currentFighterState == fighterState.searching)
        {
            currentTarget = FindClosestEnemy("Defence");

            if (currentTarget == null)
            {
                currentTarget = FindClosestWall();
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
                    if (currentPathNode < path.Capacity) // if the agent has not yet completed their path, continue to the next node
                    {
                        currentPathNode++;
                        startMovement();
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
        if (currentFighterState == fighterState.attacking)
        {
            agent.GetComponent<Animator>().SetBool("Idle", true);
            agent.GetComponent<Animator>().SetBool("Moving", false);

            attackTimer -= Time.deltaTime;

            if (attackTimer <= 0.0f)
            {
                attackTimer = attackSpeed;
                currentTarget.GetComponent<StationaryObject>().HP -= attackDamage;
                enableHealthBar(currentTarget.transform);
            }

            if (currentTarget.GetComponent<StationaryObject>().HP <= 0.0f)
            {
                currentFighterState = fighterState.searching;
            }
            //if (currentTarget == null)
            //{
            //    currentFighterState = fighterState.searching;
            //}
        }

    }


    private void startMovement()
    {
        //agent.Move(new Vector3(path[currentPathNode].x, 0.0f, path[currentPathNode].y));
        agent.SetDestination(new Vector3(path[currentPathNode].x, agent.transform.position.y, path[currentPathNode].y));


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

    protected new GameObject FindClosestEnemy(string tag)
    {
        GameObject[] gos; // an array holding the game objects of all enemies
        gos = GameObject.FindGameObjectsWithTag(tag); // finds game objects with the defence tag and loads them into an array

        GameObject closest = null; // creates another game object to hold the closest one

        Vector3 position = transform.position; //sets position to the position of the game object searching (the knight)

        List<AStar.mapNode> currentPath = new List<AStar.mapNode>(); // holds the lengh of the current path being evaluated. starts at infinity so that the initial value is lower in the initial check

        foreach (GameObject go in gos) // checks each game object "go" in th game object array
        {
            path = AStarRef.AStarSearch((int)position.x, (int)position.z, (int)go.transform.position.x, (int)go.transform.position.z);

            if (currentPath.Count == 0 || path[path.Count - 1].totalCost < currentPath[currentPath.Count - 1].totalCost) // if current path is empty, or new path is better
            {
                currentPath = path;
                closest = go;
            }
        }

        return closest; // returns a game object with the shortest path
    }

    protected new GameObject FindClosestWall()
    {
        GameObject[] gos;
        gos = GameObject.FindGameObjectsWithTag("Wall"); // finds game objects with the defence tag and loads them into an array

        GameObject closest = null; // creates another game object to hold the closest one

        Vector3 position = transform.position; //sets position to the position of the game object searching

        List<AStar.mapNode> currentPath = new List<AStar.mapNode>(); // holds the lengh of the current path being evaluated. starts at infinity so that the initial value is lower in the initial check

        foreach (GameObject go in gos) // checks each game object "go" in th game object array
        {
            path = AStarRef.AStarSearch((int)position.x, (int)position.z, (int)go.transform.position.x, (int)go.transform.position.z);

            if (currentPath.Count == 0 || path[path.Count - 1].totalCost < currentPath[currentPath.Count - 1].totalCost) // if current path is empty, or new path is better
            {
                currentPath = path;
                closest = go;
            }
        }

        return closest; // returns a game object with the shortest path
    }
}
