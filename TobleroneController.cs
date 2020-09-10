using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TobleroneController : MonoBehaviour
{
    private float rotationSpeed = 10.0f;
    private float finalRotationSpeed;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        finalRotationSpeed = rotationSpeed * Time.fixedDeltaTime;

        transform.Rotate(transform.right, finalRotationSpeed, Space.World);
    }
}
