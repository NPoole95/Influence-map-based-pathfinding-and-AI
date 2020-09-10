using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightFlicker : MonoBehaviour
{
    float maxFlickerIntensity = 4.0f;
    float minFlickerIntensity = 2.0f;
    float flickerSpeed = 3.0f;
    float finalFlickerSpeed;
    float nextIntensity;

    public Light flickeringLight;


    // Start is called before the first frame update
    void Start()
    {
        nextIntensity = (Random.Range(minFlickerIntensity, maxFlickerIntensity));
        flickeringLight = transform.GetComponentInChildren<Light>();
    }

    // Update is called once per frame
    void Update()
    {
        finalFlickerSpeed = flickerSpeed * Time.fixedDeltaTime;

        if (flickeringLight.intensity > nextIntensity - 0.05f && flickeringLight.intensity < nextIntensity + 0.05f)
        {
            nextIntensity = (Random.Range(minFlickerIntensity, maxFlickerIntensity));
        }
        else if (flickeringLight.intensity > nextIntensity)
        {
            flickeringLight.intensity -= finalFlickerSpeed;
        }
        else if (flickeringLight.intensity < nextIntensity)
        {
            flickeringLight.intensity += finalFlickerSpeed;
        }
    }
}