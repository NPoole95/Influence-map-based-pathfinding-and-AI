using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathMapCameraController : MonoBehaviour
{
    LoadDeathMapFromFile LMFF;
    private Camera camera;

    private const float cameraSpeed = 10.0f; //the speed at which the camera moves when dragged
    private const float cameraRotationSpeed = 40.0f; //the speed at which the camera moves when dragged
    private const float cameraZoomSpeed = 0.7f; //the speed at which the camera moves when dragged
    private const int LEFTCLICK = 0;

    // Update is called once per frame

    private void Awake()
    {
        transform.position = new Vector3(20.0f, 25.0f, transform.position.z);
        LMFF = GameObject.Find("LoadDeathMapFromFile").GetComponent<LoadDeathMapFromFile>();
    }
    void Update()
    {
        if (Input.GetMouseButton(LEFTCLICK)) // checks if the left mouse button is down
        {
            transform.Rotate(Input.GetAxisRaw("Mouse Y") * Time.deltaTime * cameraRotationSpeed, 0.0f, 0.0f);

            Vector3 yAxis = new Vector3(0.0f, 1.0f, 0.0f);
            transform.RotateAround(transform.position, yAxis, Input.GetAxisRaw("Mouse X") * Time.deltaTime * cameraRotationSpeed);

        }
        if (Input.GetAxis("Mouse ScrollWheel") >= 0.02f)
        {
            //transform.position += new Vector3(0.0f, Time.deltaTime * -cameraSpeed,0.0f);
            //transform.RotateAroundLocal(transform.right, -0.03f);
            if (transform.position.y > 6.0f)
            {
                transform.position += transform.forward * cameraZoomSpeed;
            }
        }
        else if (Input.GetAxis("Mouse ScrollWheel") <= -0.02f)
        {
            //transform.position += new Vector3(0.0f, Time.deltaTime * cameraSpeed, 0.0f);
            if (transform.position.y < 25.0f)
            {
                transform.position += transform.forward * -cameraZoomSpeed;
            }
            // transform.RotateAroundLocal(transform.right, 0.03f);
        }

        if (Input.GetKey(KeyCode.W)) // checks if the W key is down
        {
            transform.position += new Vector3(0.0f, 0.0f, Time.deltaTime * cameraSpeed);
        }
        if (Input.GetKey(KeyCode.S)) // checks if the S key is down
        {
            transform.position += new Vector3(0.0f, 0.0f, Time.deltaTime * -cameraSpeed);
        }
        if (Input.GetKey(KeyCode.D)) // checks if the D key is down
        {
            transform.position += new Vector3(Time.deltaTime * cameraSpeed, 0.0f, 0.0f);
        }
        if (Input.GetKey(KeyCode.A)) // checks if the A key is down
        {
            transform.position += new Vector3(Time.deltaTime * -cameraSpeed, 0.0f, 0.0f);
        }


        //////// Boundary checks/////////////
        if (transform.rotation.eulerAngles.x < 30.0f)
        {
            transform.eulerAngles = new Vector3(30.0f, transform.rotation.y, transform.rotation.z);           
        }
        else if (transform.rotation.eulerAngles.x > 65.0f)
        {
            transform.eulerAngles = new Vector3(65.0f, transform.rotation.y, transform.rotation.z);
        }

        if (transform.position.x < 0.0f)
        {
            transform.position = new Vector3(0.0f, transform.position.y, transform.position.z);
        }
        else if (transform.position.x > LMFF.mapWidth)
        {
            transform.position = new Vector3(LMFF.mapWidth, transform.position.y, transform.position.z);
        }

        //if (transform.position.y < 5.0f)
        //{
        //    transform.position = new Vector3(transform.position.x, 5.0f, transform.position.z);
        //}
        //else if (transform.position.y > 15.0f)
        //{
        //    transform.position = new Vector3(transform.position.x, 15.0f, transform.position.z);
        //}

        if (transform.position.z < 0.0f)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, 0.0f);
        }
        else if (transform.position.z > LMFF.mapHeight)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, LMFF.mapHeight);
        }
    }
}
