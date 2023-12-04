using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class LayerSorter : MonoBehaviour
{
    public int defaultSortingOrder = 50;
    SpriteRenderer spriteRenderer;

    private List<Obstacle> obstacles = new List<Obstacle>();
    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponentInParent<SpriteRenderer>();
        spriteRenderer.sortingOrder = defaultSortingOrder;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Obstacle")
        {
            Obstacle obstacle = collision.GetComponent<Obstacle>();

            if(obstacles.Count == 0 || obstacle.spriteRenderer.sortingOrder - 1 < spriteRenderer.sortingOrder)
            {
                spriteRenderer.sortingOrder = obstacle.spriteRenderer.sortingOrder - 1;
            }

            obstacles.Add(obstacle);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Obstacle")
        {
            Obstacle obstacle = collision.GetComponent<Obstacle>();
            obstacles.Remove(obstacle);

            if(obstacles.Count == 0)
            {
                spriteRenderer.sortingOrder = defaultSortingOrder;
            }
            else
            {
                obstacles.Sort();
                spriteRenderer.sortingOrder = obstacles[0].spriteRenderer.sortingOrder - 1;
            }
        }
    }
}
