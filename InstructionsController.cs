using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InstructionsController : MonoBehaviour
{
    [SerializeField]
    GameObject BaseSceneInstructions;
    [SerializeField]
    GameObject MazeSceneInstructions;

    void Awake()
    {
        MazeSceneInstructions.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void selectInstructions()
    {
        switch (this.gameObject.name)
        {
            case "MazeButton":
                MazeSceneInstructions.SetActive(true);
                BaseSceneInstructions.SetActive(false);
                break;
            case "BaseButton":
                MazeSceneInstructions.SetActive(false);
                BaseSceneInstructions.SetActive(true);
                break;
        }
    }
}
