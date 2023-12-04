using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Serializable]
    public struct EnemyInfo
    {
        public GameObject enemyPrefab;
        public int startPointLevel;
    }

    public EnemyInfo[] enemies;
    public PlayerController player;
    public GameObject nextLevel;

    public float distanceFromPlayer = 10;
    public float generationRadius = 20;
    public int waveTime = 20;

    public int initNumEnemies = 10;
    public int[] waves;
    //public int numWaves = 3;

    public GameObject boss;

    public TextMeshProUGUI waveTimer;
    public TextMeshProUGUI enemiesLeft;

    public WFCGenerator mapGenerator;

    private List<GameObject> remainingEnemies = new List<GameObject>();

    private int currentWave = 0;

    private bool defeatedBoss = false;
    
    // Start is called before the first frame update
    void Awake()
    {
        nextLevel.SetActive(false);
        mapGenerator.OnCompleted += () =>
        {
            StartCoroutine(SpawnEnemies());
        };
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GenerateEnemies(Vector2 center, float radius, int numEnemies)
    {
        if (defeatedBoss) return;

        waveTimer.text = "";
        if(currentWave > waves.Length - 1)
        {
            Vector2 pos = PickEnemySpawnPos(center, radius);
            GameObject enemy = Instantiate(boss, pos, Quaternion.identity);
            remainingEnemies.Add(enemy);
            EnemyController enemyController = enemy.GetComponent<EnemyController>();
            enemyController.player = player.transform;
            enemyController.OnDeath += OnBossDeath;
            return;
        }

        for (int i = 0; i < numEnemies; i++)
        {
            Vector2 pos = PickEnemySpawnPos(center, radius);
            GameObject enemy = Instantiate(PickEnemyToSpawn(), pos, Quaternion.identity);
            remainingEnemies.Add(enemy);
            EnemyController enemyController = enemy.GetComponent<EnemyController>();
            enemyController.player = player.transform;
            enemyController.OnDeath += OnEnemyDeath;
        }

        currentWave++;

        enemiesLeft.text = $"Enemies Left: {remainingEnemies.Count}";
    }

    private Vector2 PickEnemySpawnPos(Vector2 center, float radius)
    {
        Bounds smallerBounds = GameManager.Instance.mapBounds;
        smallerBounds.Expand(-2);
        Vector2 pos = Utils.SpawnInCircle(center, distanceFromPlayer, radius);

        for(int i = 0; i < 50 && !smallerBounds.Contains(pos); i++)
        {
            pos = Utils.SpawnInCircle(center, distanceFromPlayer, radius);
        }

        if(!smallerBounds.Contains(pos))
        {
            pos = new Vector2(UnityEngine.Random.Range(smallerBounds.min.x, smallerBounds.max.x),
                UnityEngine.Random.Range(smallerBounds.min.y, smallerBounds.max.y));
        }

        return pos;
    }

    private GameObject PickEnemyToSpawn()
    {
        GameObject[] enemiesToSpawn = enemies.Where(enemy => enemy.startPointLevel <= player.score).Select(enemy => enemy.enemyPrefab).ToArray();
        return enemiesToSpawn[UnityEngine.Random.Range(0, enemiesToSpawn.Length)];
    }

    IEnumerator SpawnEnemies()
    {
        int timeLeft = waveTime;
        waveTimer.text = $"Next wave in: {timeLeft}s";
        enemiesLeft.text = "";

        while (timeLeft > 0)
        {
            yield return new WaitForSeconds(1);
            timeLeft--;
            waveTimer.text = $"Next wave in: {timeLeft}s";
        }

        GenerateEnemies(player.transform.position, generationRadius, currentWave < waves.Length ? waves[currentWave] : 0);
    }

    private void OnEnemyDeath(GameObject enemy)
    {
        remainingEnemies.Remove(enemy);
        enemiesLeft.text = $"Enemies Left: {remainingEnemies.Count}";

        if (remainingEnemies.Count == 0)
        {
            StartCoroutine(SpawnEnemies());
        }
    }
    private void OnBossDeath(GameObject enemy)
    {
        remainingEnemies.Remove(enemy);
        defeatedBoss = true;
        nextLevel.SetActive(true);
        enemiesLeft.text = "";
        waveTimer.text = "";
    }
}
