using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoadingHandler : MonoBehaviour
{
    public GameObject loadingImage;
    public Slider loadingSlider;
    public List<GameObject> ignoreObjects;
    public WFCGenerator mapGenerator;

    private Dictionary<GameObject, bool> goActive = new Dictionary<GameObject, bool>();
    // Start is called before the first frame update
    void Awake()
    {
        loadingImage.SetActive(true);
        ignoreObjects.Add(loadingImage.transform.parent.gameObject);

        foreach (GameObject obj in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            if (ignoreObjects.Contains(obj)) continue;

            goActive.Add(obj, obj.activeSelf);

            obj.SetActive(false);
        }

        mapGenerator.OnCompleted += () =>
        {
            foreach (GameObject obj in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                if (goActive.ContainsKey(obj))
                {
                    obj.SetActive(goActive[obj]);
                }
                else
                {
                    obj.SetActive(true);
                }
            }
            loadingImage.SetActive(false);
        };

        loadingSlider.value = 0;

        mapGenerator.OnProgress += (float p) =>
        {
            loadingSlider.value = p;
        };
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
