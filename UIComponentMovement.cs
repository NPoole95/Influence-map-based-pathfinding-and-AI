using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIComponentMovement : MonoBehaviour
{
    public GameObject knightImage;
    public GameObject giantImage;
    public GameObject selectionImage;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            selectionImage.transform.position = knightImage.transform.position;
        }
        else if (Input.GetKeyDown(KeyCode.G))
        {
            selectionImage.transform.position = giantImage.transform.position;
        }
    }
}
