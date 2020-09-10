using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneController : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject bomb;
    float timeCounter;

    float speed; // the speed at which the plane will fly
    float turnSpeed = 5.0f; // in degrees
    float finalSpeed;
    float finalTurnSpeed;

    private float attackTimer = 0.0f;
    private float attackSpeed = 6.0f; // the interval in seconds between attacks


    float width; // the radius of the circl that the plane will fly in

    float startingX;
    //float startingY;
    float startingZ;

    Vector3 OldPos; // the position of the plane in the last frame
    Vector3 NewPos; // the position of the plane this frame
    LoadMapFromFile LMFF;

    void Start()
    {
        LMFF = GameObject.Find("LoadMapFromFile").GetComponent<LoadMapFromFile>();
        timeCounter = Random.Range(0.0f, 6.0f); // spawns the plane in a random position 
        speed = 0.25f;
        width = 10;

        startingX = transform.position.x;
        startingZ = transform.position.z;
    }

    // Update is called once per frame
    void Update()
    {
        finalSpeed = speed * Time.deltaTime;
        finalTurnSpeed = turnSpeed * Time.deltaTime;

        OldPos = transform.position;
        updatePosition();

        NewPos = transform.position;

        Vector3 facingVector = new Vector3( NewPos.x - OldPos.x, transform.position.y, NewPos.z - OldPos.z ) ;

        transform.LookAt(NewPos + facingVector);


        //float diff = Vector3.Distance(currentTarget.transform.position, transform.position);
        //if (diff > attackRadius)
        //{
        //    currentDefenceState = defenceState.searching;
        //}

        attackTimer -= Time.deltaTime;

        if (attackTimer <= 0.0f)
        {
            Vector3 bombPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z);

            GameObject projectile = Instantiate(bomb, bombPosition, Quaternion.Euler(180.0f, 0.0f, 0.0f));
            BombController bombController = projectile.GetComponent<BombController>();

            attackTimer = attackSpeed;
            //currentTarget.GetComponent<MovingObject>().HP -= attackDamage;
            //enableHealthBar(currentTarget.transform);
        }

    }

    void updatePosition()
    {
        timeCounter += Time.deltaTime * speed;

        float x = (Mathf.Cos(timeCounter) * width) + startingX;
        float y = transform.position.y;
        float z = (Mathf.Sin(timeCounter) * width) + startingZ;

        transform.position = new Vector3(x, y, z);
    }
}

