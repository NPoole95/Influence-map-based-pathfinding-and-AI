using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class DeathMapTowerController : StationaryObject
{

    private float attackRadius = 4.0f;
    private const int MAXHP = 200;
    private float attackTimer = 0.0f;
    private float attackSpeed = 1.5f; // the interval in seconds between attacks
    private float attackDamage = 25.0f; // the amount of damage dealt per attack
    private GameObject currentTarget;

    int layer_mask;

    private Transform camera; // used for the camera lookat 
    public GameObject healthBar;
    public Slider healthBarSlider;

    // Start is called before the first frame update
    protected override void Awake()
    {
        layer_mask = LayerMask.GetMask("Wall", "Fighter");
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

        if (currentTarget == null || currentTarget.GetComponent<MovingObject>().HP <= 0.0f)
        {
            currentDefenceState = defenceState.searching;
        }

        if (currentDefenceState == defenceState.searching)
        {
            currentTarget = FindTargetInRange();
        }
        if (currentDefenceState == defenceState.attacking)
        {
            if (currentTarget == null || !CanSeeFighter(currentTarget.transform))
            {
                Debug.Log("target lost");
                currentDefenceState = defenceState.searching;
            }
            else
            {
                attackTimer -= Time.deltaTime;
                if (attackTimer <= 0.0f)
                {
                    attackTimer = attackSpeed;
                    currentTarget.GetComponent<MovingObject>().HP -= attackDamage;
                    enableHealthBar(currentTarget.transform);
                    Debug.Log("Pew");
                }
            }
        }
        if (currentDefenceState == defenceState.destroyed)
        {
            transform.gameObject.tag = "Untagged";
            // call influence map update
            Destroy(gameObject);

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
        RaycastHit hit;

        Vector3 position = transform.position;
        foreach (GameObject go in gos) // checks each game object "go" in th game object array
        {
            if (CanSeeFighter(go.transform))
            {
                Debug.Log("target spotted");
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

    private bool CanSeeFighter(Transform target)
    {
        RaycastHit hit;
        Vector3 position = transform.position;
        Vector3 rayStart = new Vector3(position.x, 0.8f, position.z);
        Vector3 rayend = new Vector3(target.position.x, 0.8f, target.position.z);

        if (Physics.Linecast(rayStart, rayend, out hit, layer_mask))
        {
            float diff = Mathf.Abs((Mathf.Abs(target.position.x) - Mathf.Abs(position.x)) + (Mathf.Abs(target.position.z) - Mathf.Abs(position.z))); // gets the distance between the found object and the object doing the search
            if (hit.transform.tag == "Fighter - Ground" && diff < attackRadius)
            {
                currentDefenceState = defenceState.attacking;
                return true;
            }
        }
        return false;
    }
}

