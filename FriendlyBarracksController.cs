using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class FriendlyBarracksController : StationaryObject
{
    private const int MAXHP = 200;

    private Transform camera; // used for the camera lookat 
    public Transform knight;

    public GameObject healthBar;
    public Slider healthBarSlider;

    public GameObject productionBar;
    public Slider productionBarSlider;

    private float productionTimerLimit;
    private float productionTimer;

    // Start is called before the first frame update
    protected override void Awake()
    {
        HP = MAXHP;
        productionTimerLimit = 10.0f;
        currentDefenceState = defenceState.attacking;
        camera = Camera.main.transform;
        healthBarSlider.value = CalculateHealth();
        base.Awake();
    }

    // Update is called once per frame
    void Update()
    {

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
                Instantiate(knight, position, Quaternion.identity);
            }
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

