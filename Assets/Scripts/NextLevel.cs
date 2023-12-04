using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NextLevel : MonoBehaviour
{
    public string sceneName;

    public void LoadNextLevel()
    {
        SceneManager.LoadSceneAsync(sceneName);
    }
}
