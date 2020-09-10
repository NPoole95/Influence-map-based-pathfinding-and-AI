using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombController : MonoBehaviour
{
    float speed = 5.0f;
    private float attackDamage = 30.0f; // the amount of damage dealt per attack
    float bombRadius = 2.0f;

    bool bombExploded = false;
    float deleteTimer = 0.0f;

    public GameObject explosion;
    public AudioSource audioSource;

    // Update is called once per frame
    void Update()
    {
        float moveSpeed = speed * Time.deltaTime; // calculate distance to move
        transform.position -= new Vector3(0.0f, moveSpeed, 0.0f);

        if (transform.position.y <= 0.001f && bombExploded == false)
        {
            GameObject[] gos; // an array holding the game objects of all enemies
            gos = GameObject.FindGameObjectsWithTag("Fighter - Ground"); // finds game objects with the "Fighter - Ground" tag and loads them into an array
            Vector3 position = transform.position; //sets position to the position of the game object searching

            foreach (GameObject go in gos) // checks each game object "go" in th game object array
            {
                //float diff = Mathf.Abs((Mathf.Abs(go.transform.position.x) - Mathf.Abs(position.x)) + (Mathf.Abs(go.transform.position.z) - Mathf.Abs(position.z))); // gets the distance between the found object and the object doing the search
                float diff = Vector3.Distance(go.transform.position, transform.position);
                if (diff < bombRadius)
                {
                    go.GetComponent<MovingObject>().HP -= attackDamage;
                    enableHealthBar(go.transform);
                }
            }
            explosion = Instantiate(explosion, transform.position, Quaternion.identity);
            bombExploded = true;
            audioSource.Play();
        }

        if(bombExploded == true)
        {
            deleteTimer += Time.deltaTime;
            if (deleteTimer > 2.0f)
            {
                Destroy(explosion);
                Destroy(gameObject);
            }
        }
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
