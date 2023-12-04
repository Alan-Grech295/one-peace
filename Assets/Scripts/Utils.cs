using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utils
{
    public static Vector2 SpawnInCircle(Vector2 center, float innerRadius, float outerRadius)
    {
        float dist = Random.Range(innerRadius, outerRadius);
        float angle = Random.Range(0, 2 * Mathf.PI);

        return center + new Vector2(Mathf.Sin(angle), Mathf.Cos(angle)) * dist;
    }
}
