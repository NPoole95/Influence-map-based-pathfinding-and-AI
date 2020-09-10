using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class BarracksController : StationaryObject
{
    private const int MAXHP = 200;

    private Transform camera; // used for the camera lookat 
    public Transform Defender;

    public int numberOfDefenders;

    public GameObject healthBar;
    public Slider healthBarSlider;

    public GameObject productionBar;
    public Slider productionBarSlider;

    private float productionTimerLimit;
    private float productionTimer;

    public InfluenceMapController imController;

    // Start is called before the first frame update
    protected override void Awake()
    {
        imController = GameObject.Find("InfluenceMapController").GetComponent<InfluenceMapController>();
        HP = MAXHP;
        numberOfDefenders = 0;
        productionTimerLimit = 10.0f;
        currentDefenceState = defenceState.attacking;
        camera = Camera.main.transform;
        healthBarSlider.value = CalculateHealth();
        base.Awake();
    }

    // Update is called once per frame
    void Update()
    {

        if( numberOfDefenders >= 5)
        {
            currentDefenceState = defenceState.idle;
        }
        else
        {
            currentDefenceState = defenceState.attacking;
        }


        healthBarSlider.value = CalculateHealth();
        healthBarSlider.transform.LookAt(camera);

        productionBarSlider.value = CalculateProduction();
        productionBarSlider.transform.LookAt(camera);

        if (HP <= 0)
        {         
            currentDefenceState = defenceState.destroyed;
        }
        else if (HP < MAXHP)
        {
            healthBar.SetActive(true);
        }

        if (currentDefenceState == defenceState.attacking)
        {
            productionTimer += Time.deltaTime;

            if (productionTimer >= productionTimerLimit)
            {
                productionTimer = 0.0f;
                // instantiate fighter     
                Vector3 position = transform.position;
                position.z -= 1.0f;
                Instantiate(Defender, position, Quaternion.identity);
                ++numberOfDefenders;

            }
        }
        if (currentDefenceState == defenceState.destroyed)
        {
            transform.gameObject.tag = "Untagged";
            // call influence map update
            imController.removeDangerSource((int)transform.position.x, (int)transform.position.z);
            Destroy(gameObject);

        }
    }
    float CalculateHealth()
    {
        return HP / MAXHP;
    }
    float CalculateProduction()
    {
        return productionTimer / productionTimerLimit;
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

