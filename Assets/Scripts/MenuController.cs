using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public void GameStart()
    {
        SceneManager.LoadScene("Scenes/Stage1");
    }

    public void Quit()
    {
        Application.Quit();
    }
}
