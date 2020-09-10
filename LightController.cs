using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightController : MonoBehaviour
{
    private float rotationSpeed = 0.1f;
    private float finalRotationSpeed;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        finalRotationSpeed = rotationSpeed * Time.fixedDeltaTime;

        transform.RotateAround(transform.forward, finalRotationSpeed);
    }
}