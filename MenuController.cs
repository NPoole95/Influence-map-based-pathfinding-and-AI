using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public void selectScene()
        {
        switch(this.gameObject.name)
        {
            case "MazeSceneButton":
                SceneManager.LoadScene("MazeScene");
                break;
            case "BaseSceneButton":
                SceneManager.LoadScene("MapFromFileTest");
                break;
            case "HowToPlayButton":
                SceneManager.LoadScene("HowToPlayScene");
                break;
            case "QuitButton":
                Application.Quit();
                break;
        }
    }
}
