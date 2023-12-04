using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    private AsyncOperation loadOperation;
    public void Play()
    {
        SceneManager.LoadScene("Play");
    }

    public void Options()
    {
        SceneManager.LoadScene("Options");
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("Start Scene");
    }

    public void StartGame()
    {
        loadOperation = SceneManager.LoadSceneAsync("Level1");
    }
}
