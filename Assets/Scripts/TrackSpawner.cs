using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackSpawner : MonoBehaviour
{
    public GameObject trackPrefab;
    public float moveDist = 0.2f;
    public float trackVisibilityTime = 10;

    private Vector2 pastPos;

    private Queue<GameObject> trackPool = new Queue<GameObject>();
    private Queue<Tuple<GameObject, float>> visibleTracks = new Queue<Tuple<GameObject, float>>();
    private const int numTracks = 50;
    // Start is called before the first frame update
    void Start()
    {
        GameObject trackPoolGO = new GameObject();
        for(int i = 0; i < numTracks; i++)
        {
            GameObject track = Instantiate(trackPrefab);
            track.SetActive(false);
            track.transform.SetParent(trackPoolGO.transform);
            trackPool.Enqueue(track);
        }
    }

    // Update is called once per frame
    void Update()
    {
        while(visibleTracks.Count > 0 && visibleTracks.Peek().Item2 + trackVisibilityTime <= Time.time)
        {
            GameObject track = visibleTracks.Dequeue().Item1;
            track.SetActive(false);
            trackPool.Enqueue(track);
        }

        if((pastPos - (Vector2)transform.position).sqrMagnitude > moveDist * moveDist)
        {
            pastPos = (Vector2)transform.position;
            if(trackPool.Count == 0)
            {
                trackPool.Enqueue(visibleTracks.Dequeue().Item1);
            }

            GameObject track = trackPool.Dequeue();
            Animator anim = track.GetComponent<Animator>();
            anim.speed = 1f / trackVisibilityTime;
            anim.Play("TrackAnimation");
            track.SetActive(true);
            track.transform.position = transform.position;
            track.transform.rotation = transform.rotation;
            visibleTracks.Enqueue(new Tuple<GameObject, float>(track, Time.time));
        }
    }
}
