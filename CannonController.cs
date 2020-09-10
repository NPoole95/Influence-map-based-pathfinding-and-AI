using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class CannonController : StationaryObject
{
    public LoadMapFromFile LMFF;
    private float attackRadius = 3.0f;
    private const int MAXHP = 200;
    private float attackTimer = 0.0f;
    private float attackSpeed = 1.5f; // the interval in seconds between attacks
    private float attackDamage = 15.0f; // the amount of damage dealt per attack
    private GameObject currentTarget;

    public GameObject cannonball;
    public AudioSource audioSource;
    private Transform camera; // used for the camera lookat 
    public GameObject healthBar;
    public Slider healthBarSlider;
    public InfluenceMapController imController;

    // Start is called before the first frame update
    protected override void Awake()
    {
        imController = GameObject.Find("InfluenceMapController").GetComponent<InfluenceMapController>();
        LMFF = GameObject.Find("LoadMapFromFile").GetComponent<LoadMapFromFile>();
        HP = MAXHP;
        currentDefenceState = defenceState.searching;
        camera = Camera.main.transform;
        healthBarSlider.value = CalculateHealth();
        base.Awake();
    }

    // Update is called once per frame
    void Update()
    {
        healthBarSlider.value = CalculateHealth();
        healthBarSlider.transform.LookAt(camera);

        if (HP <= 0)
        {
            currentDefenceState = defenceState.destroyed;
        }
        else if (HP < MAXHP)
        {
            healthBar.SetActive(true);
        }
        ////////////////////////////
        if (currentDefenceState == defenceState.searching)
        {
            currentTarget = FindTargetInRange();
        }

        ///////////////////////////////
        if (currentDefenceState == defenceState.attacking)
        {
            if (currentTarget == null)
            {
                currentDefenceState = defenceState.searching;
            }
            else
            {

                transform.LookAt(currentTarget.transform);

                float diff = Vector3.Distance(currentTarget.transform.position, transform.position);
                if (diff > attackRadius)
                {
                    currentDefenceState = defenceState.searching;
                }

                attackTimer -= Time.deltaTime;

                if (attackTimer <= 0.0f)
                {
                    Vector3 cannonballPosition = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);
                    GameObject projectile = Instantiate(cannonball, cannonballPosition, Quaternion.identity);
                    CannonballController cannonballController = projectile.GetComponent<CannonballController>();
                    audioSource.Play();
                    cannonballController.target = currentTarget;

                    attackTimer = attackSpeed;
                    //currentTarget.GetComponent<MovingObject>().HP -= attackDamage;
                    //enableHealthBar(currentTarget.transform);
                }
            }
        }

        //////////////////////////////////
        if (currentDefenceState == defenceState.destroyed)
        {
            transform.gameObject.tag = "Untagged";
            // call influence map update
            imController.removeDangerSource((int)transform.position.x, (int)transform.position.z);
            Destroy(gameObject);
            LMFF.map[(int)transform.position.x, (int)transform.position.z] = 0;
        }
    }
    float CalculateHealth()
    {
        return HP / MAXHP;
    }

    protected GameObject FindTargetInRange()
    {
        GameObject[] gos; // an array holding the game objects of all enemies
        gos = GameObject.FindGameObjectsWithTag("Fighter - Ground"); // finds game objects with the "Fighter - Ground" tag and loads them into an array
        Vector3 position = transform.position; //sets position to the position of the game object searching

        foreach (GameObject go in gos) // checks each game object "go" in th game object array
        {
            //float diff = Mathf.Abs((Mathf.Abs(go.transform.position.x) - Mathf.Abs(position.x)) + (Mathf.Abs(go.transform.position.z) - Mathf.Abs(position.z))); // gets the distance between the found object and the object doing the search
            float diff = Vector3.Distance(go.transform.position, transform.position);
            if (diff < attackRadius)
            {
                Debug.Log(diff);
                currentDefenceState = defenceState.attacking;
                return go;
            }
        }
        return null; // returns a game object with the shortest path
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
}
