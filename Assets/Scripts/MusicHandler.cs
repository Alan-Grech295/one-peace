using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicHandler : MonoBehaviour
{
    [Serializable]
    public struct SceneMusic
    {
        public string name;
        public AudioClip clip;
    }

    public SceneMusic[] sceneMusic;

    Dictionary<string, AudioClip> sceneMusicDict = new Dictionary<string, AudioClip>();
    AudioSource audioSource;

    static MusicHandler Instance = null;

    // Start is called before the first frame update
    void Start()
    {
        if(Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        foreach (var sceneMusic in sceneMusic)
        {
            sceneMusicDict.Add(sceneMusic.name, sceneMusic.clip);
        }

        audioSource = GetComponent<AudioSource>();
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnLoadScene;
    }

    void OnLoadScene(Scene scene, LoadSceneMode mode)
    {
        if(sceneMusicDict.ContainsKey(scene.name))
        {
            if(audioSource.clip != sceneMusicDict[scene.name])
            {
                audioSource.clip = sceneMusicDict[scene.name];
                audioSource.Play();
            }
        }
        else
        {
            audioSource.Stop();
        }
    }
}
