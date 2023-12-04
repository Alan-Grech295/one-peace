using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [System.Serializable]
    public struct Object
    {
        public GameObject obstacle;
        public int placeChance;
        public Vector2 rotation;
    }

    public Object[] obstacles;
    [Range(0, 100)]
    public float placeChance = 80f;

    public float placeRadius = 10;

    private List<GameObject> objects = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        Place();
    }

    public void Place()
    {
        int sumChance = obstacles.Select(o => o.placeChance).Sum();

        for (float y = GameManager.Instance.mapBounds.min.y; y < GameManager.Instance.mapBounds.max.y; y += placeRadius)
        {
            for(float x = GameManager.Instance.mapBounds.min.x; x < GameManager.Instance.mapBounds.max.x; x += placeRadius)
            {
                if (Random.Range(0, 100) > placeChance) continue;

                Vector2 placePos = new Vector2(x + Random.Range(0f, placeRadius), y + Random.Range(0f, placeRadius));

                int place = Random.Range(0, sumChance + 1);
                Object placeObj = obstacles[obstacles.Length - 1];
                foreach (Object o in obstacles)
                {
                    place -= o.placeChance;
                    if(place <= 0)
                    {
                        placeObj = o;
                        break;
                    }
                }

                GameObject obstacle = Instantiate(placeObj.obstacle, placePos, Quaternion.Euler(0, 0, Random.Range(placeObj.rotation.x, placeObj.rotation.y)));
                objects.Add(obstacle);
            }
        }
    }

    public void Clear()
    {
        foreach(GameObject go in objects)
        {
            Destroy(go);
        }

        objects.Clear();
    }
}
