using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class MovingObject : MonoBehaviour // set to abstract so they must be implemented in derived class
{
    protected enum fighterState { searching, moving, attacking, dead };
    protected fighterState currentFighterState = fighterState.searching;

    protected GameObject currentTarget;
    public NavMeshAgent agent;

    [HideInInspector]
    protected bool pathPosible = false; // boolean used to show if a pass is possible. Eg if no path to defence is possible, attack walls.
    public NavMeshPath navMeshPath;

    public float HP;

    public virtual void Awake()
    {
        navMeshPath = new NavMeshPath();
    }
    public virtual void Update()
    {
        //if (currentFighterState == fighterState.searching)
        //{
        //    currentTarget = FindClosestEnemy("Defence");

        //    if (currentTarget == null)
        //    {
        //        currentTarget = FindClosestWall();
        //        currentFighterState = fighterState.moving;
        //    }
        //    else
        //    {
        //        currentFighterState = fighterState.moving;
        //    }
        //}
        //if (currentFighterState == fighterState.moving)
        //{

        //  agent.SetDestination(currentTarget.transform.position); // sets the agents destination to the point that was clicked
               
        //  if (agent.remainingDistance > agent.stoppingDistance)
        //  {
        //      if (agent.CompareTag("Fighter - Ground"))
        //      {
        //          agent.GetComponent<Animator>().SetTrigger("Moving");
        //      }
        //  }
        //  else
        //  {
        //      agent.GetComponent<Animator>().SetTrigger("Idle");
        //       currentFighterState = fighterState.attacking;
        //  }
        //}
        //if (currentFighterState == fighterState.attacking)
        //{

        //}
    }

    protected GameObject FindClosestEnemy(string tag)
    {
        GameObject[] gos; // an array holding the game objects of all enemies
        gos = GameObject.FindGameObjectsWithTag(tag); // finds game objects with the defence tag and loads them into an array

        GameObject closest = null; // creates another game object to hold the closest one

        Vector3 position = transform.position; //sets position to the position of the game object searching

        float currentPathDistance = Mathf.Infinity; // holds the lengh of the current path being evaluated. starts at infinity so that the initial value is lower in the initial check
        float pathLength = 0.0f; // the length of the shortest path found
        foreach (GameObject go in gos) // checks each game object "go" in th game object array
        {
            Vector3 diff = go.transform.position - position; // gets the distance between the found object and the object doing the search
 
           agent.CalculatePath((go.transform.position), navMeshPath);
           if (checkPath(go) == true)
           {
               pathLength = CalculatePathLength(go.transform.position, agent.path);

               if (pathLength < currentPathDistance)
               {
                   currentPathDistance = pathLength;
                   closest = go;
               }
           }
        }
        return closest; // returns a game object with the shortest path
    }

    protected GameObject FindClosestWall()
    {
        GameObject[] gos;
        gos = GameObject.FindGameObjectsWithTag("Wall"); // finds game objects with the defence tag and loads them into an array
        GameObject closest = null; // creates another game object to hold the closest one
        float distance = Mathf.Infinity;
        Vector3 position = transform.position; //sets position to the position of the game object searching
        foreach (GameObject go in gos) // checks each game object "go" in th game object array
        {
            Vector3 diff = go.transform.position - position; // gets the distance between the found object and the object doing the search

            float curDistance = diff.sqrMagnitude;
            if (curDistance < distance) //checks if the distance between these two is shorter than the previous shortest distance
            {   
               closest = go;
               distance = curDistance;     
            }
        }
        return closest; // returns a game object with the shortest path
    }

    private bool checkPath(GameObject go)
    {
        agent.CalculatePath((go.transform.position), navMeshPath);

        if (navMeshPath.status == NavMeshPathStatus.PathComplete)
        {
            return true;
        }
       
         return false;
        
    }

    float CalculatePathLength(Vector3 targetPosition, NavMeshPath path)
    {

        // Create an array of points which is the length of the number of corners in the path + 2.
        Vector3[] pathWayPoints = new Vector3[path.corners.Length + 2];

        // The first point is the agent's position.
        pathWayPoints[0] = transform.position;

        // The last point is the target's position.
        pathWayPoints[pathWayPoints.Length - 1] = targetPosition;

        // The points inbetween are the corners of the path.
        for (int i = 0; i < path.corners.Length; i++)
        {
            pathWayPoints[i + 1] = path.corners[i];
        }

        // Create a float to store the path length that is by default 0.
        float pathLength = 0;

        // Increment the path length by an amount equal to the distance between each waypoint and the next.
        for (int i = 0; i < pathWayPoints.Length - 1; i++)  
        {
            pathLength += Vector3.Distance(pathWayPoints[i], pathWayPoints[i + 1]);
        }

        return pathLength;
    }
}
