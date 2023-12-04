using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FadeScript : MonoBehaviour
{
    public bool fadeOut = true;
    public string sceneName;

    Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        animator.Play("FadeAnimation");
        animator.SetBool("reverse", !fadeOut);

        if (sceneName != "")
            StartCoroutine(LoadScene());
    }

    IEnumerator LoadScene()
    {
        yield return new WaitForSeconds(1);
        SceneManager.LoadSceneAsync(sceneName);
    }
}
