using UnityEngine;
using UnityEngine.AI;
public class OnClickController : MonoBehaviour
{
    public Camera cam; // creates a referenc to the camera
    public NavMeshAgent agent; // gets a reference to the agent

   
    

    public const int LEFTMOUSE = 0;
    public const int RIGHTMOUSE = 1;
    public const int MIDDLEMOUSE = 2;


    // Update is called once per frame
    void Update()
    {
        //if (Input.GetMouseButtonDown(LEFTMOUSE)) // checks if the left mouse button has been clicked
        //{
        //    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);  //cam.ScreenPointToRay(Input.mousePosition); // gets the current mouse position and fires a ray in that direction from the camera
        //    RaycastHit hit;


        //    if ( Physics.Raycast(ray, out hit)) // checks if the ray hits something
        //    {
        //        agent.SetDestination(hit.point); // sets the agents destination to the point that was clicked
        //    }
        //}


        //if (agent.remainingDistance > agent.stoppingDistance)
        //{
        //    if(agent.CompareTag("Fighter - Ground"))
        //    {
        //        agent.GetComponent<Animator>().SetTrigger("Moving");
        //    }
        //}
        //else
        //{
        //    agent.GetComponent<Animator>().SetTrigger("Idle");
        //}
        
    }
}
