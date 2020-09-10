using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StationaryObject : MonoBehaviour
{
    public enum defenceState { idle, attacking, searching, destroyed };
    public defenceState currentDefenceState = defenceState.idle;

    Vector3 cameraOrientation = new Vector3 (0.0f, 1.0f, -100000.0f);

    public float HP;

    // Start is called before the first frame update
    protected virtual void Awake()
    {
        foreach (Transform child in transform)
        {
            if (child.CompareTag("HealthBar"))
            {
                child.transform.LookAt(cameraOrientation);  
               
            }
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
