using System.Collections;
using System.Collections.Generic; // allows the use of lists
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DeathMapGameManager : MonoBehaviour
{
    public static DeathMapGameManager instance = null; // declared as static so that the variable will belong to the class and there is only 1.
    public GameObject loadInMap;
    public const int LEFTMOUSE = 0;
    public const int RIGHTMOUSE = 1;
    public const int MIDDLEMOUSE = 2;

    public Transform Knight;

    Color colour;

    private enum fighter {knight, giant};
    fighter fighterSelect = fighter.giant;





    // Start is called before the first frame update
    void Awake()
    {
        if (instance == null) // checks if the game manager has already been created
        {
            instance = this; // if not, it is assigned to this instance of the game manager
            //loadInMap = GameObject.FindObjectOfType(typeof(LoadMapFromFile)) as LoadMapFromFile; // calls the load map function from the LoadMapFromFile script
            loadInMap = GameObject.FindWithTag("LoadMap");

        }
       else if (instance != this) //if the instamce opf the game object is not this one
        {
            Destroy(gameObject); // we destroy this the ensure we dont have 2 instances of the game manager
        }
       // DontDestroyOnLoad(gameObject); // when we load a new scene, all objects in the hierarchy are normally destroyed.
                                       // because we want to keep track of the score between scenes, we will do this later instead
    }

    public void GameOver()
    {

    }

    
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            fighterSelect = fighter.knight;
            
        }
        //else if (Input.GetKeyDown(KeyCode.G))
        //{
        //    fighterSelect = fighter.giant;
           
        //}

        if (Input.GetMouseButtonDown(RIGHTMOUSE))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);  //cam.ScreenPointToRay(Input.mousePosition); // gets the current mouse position and fires a ray in that direction from the camera
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit)) // checks if the ray hits something
            {
                if (hit.transform.tag == "Terrain")
                {
                    Vector3 position;
                    position.x = hit.point.x;
                    position.y = 0.001f;
                    position.z = hit.point.z;
                    // Instantiate(knight, hit.point, Quaternion.identity);  // Instantiate the knight at the position and rotation of this transform
                    switch (fighterSelect)
                    {
                        //case fighter.giant:
                        //    Instantiate(Resources.Load("TrollGiant"), position, Quaternion.identity);
                        //    break;
                        case fighter.knight:
                            Instantiate(Knight, position, Quaternion.identity);
                            break;
                    }
                    
                }
            }
        }
    }
}
