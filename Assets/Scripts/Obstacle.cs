using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour, IComparable<Obstacle>
{
    [HideInInspector]
    public SpriteRenderer spriteRenderer;
    public int maxSortOrder = 199;

    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        float t = (transform.position.y + GameManager.Instance.mapBounds.center.y + GameManager.Instance.mapBounds.extents.y) / GameManager.Instance.mapBounds.size.y;
        spriteRenderer.sortingOrder = (int)(maxSortOrder * (1 - t));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public int CompareTo(Obstacle other)
    {
        return spriteRenderer.sortingOrder.CompareTo(other.spriteRenderer.sortingOrder);
    }
}
