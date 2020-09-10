using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonballController : MonoBehaviour
{
    public GameObject target;
    float speed = 5.0f;
    private float attackDamage = 25.0f; // the amount of damage dealt per attack

    // Update is called once per frame
    void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
        }
        else
        {
            transform.LookAt(target.transform);

            float moveSpeed = speed * Time.deltaTime; // calculate distance to move
            transform.position = Vector3.MoveTowards(transform.position, target.transform.position, moveSpeed);


            if (Vector3.Distance(transform.position, target.transform.position) < 0.001f)
            {
                target.GetComponent<MovingObject>().HP -= attackDamage;
                enableHealthBar(target.transform);                
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
