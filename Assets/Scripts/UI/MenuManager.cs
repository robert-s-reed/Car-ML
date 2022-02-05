using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public void LoadScene(string sceneName)
    {
        //Load the scene corresponding to the parameter name:
        SceneManager.LoadScene(sceneName);
    }

    public void ExitProgram()
    {
        Application.Quit(); //Close program
    }
}
