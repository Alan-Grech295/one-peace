using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class Dialogue : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI textComponent;
    [SerializeField] string[] lines;
    [SerializeField] float textSpeed = 0.07f;
    public float delay = 1;
    public bool endScene = true;

    private int index;

    // Start is called before the first frame update
    void Start()
    {
        textComponent.text = string.Empty;
        for(int i = 0; i < lines.Length; i++)
        {
            lines[i] = lines[i].Replace("\\n", "\n");
        }
        StartCoroutine(StartDialogue());
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (textComponent.text == lines[index])
            {
                NextLine();
            }
            else
            {
                StopAllCoroutines();
                textComponent.text = lines[index]; 
            }
        }
    }

    IEnumerator StartDialogue()
    {
        index = 0;

        yield return new WaitForSeconds(delay);
        //Debug.Log("Text Speed: " + textSpeed);
        StartCoroutine(TypeLine());
    }

    IEnumerator TypeLine()
    {
        foreach (char c in lines[index].ToCharArray())
        {
            textComponent.text += c;
            yield return new WaitForSeconds(textSpeed);
        }
    }

    void NextLine()
    {
        if (index < lines.Length - 1)
        {
            index++;
            textComponent.text = string.Empty;
            StartCoroutine (TypeLine());
        }
        else if(endScene)
        {
            gameObject.SetActive(false);
            SceneManager.LoadScene("End Scene");
        }
    }
}
