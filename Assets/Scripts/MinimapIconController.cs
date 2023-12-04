using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapIconController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float cameraDist = Camera.main.orthographicSize * 2;
        if(Vector2.Distance(Camera.main.transform.position, transform.parent.position) >= cameraDist - GameManager.Instance.minimapIconPadding)
        {
            transform.position = (transform.parent.position - Camera.main.transform.position).normalized * (cameraDist - GameManager.Instance.minimapIconPadding) + Camera.main.transform.position;
        }
        else
        {
            transform.localPosition = Vector3.zero;
        }
    }
}
