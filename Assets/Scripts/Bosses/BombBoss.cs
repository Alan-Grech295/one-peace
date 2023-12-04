using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombBoss : EnemyController
{
    public GameObject bombPrefab;
    public GameObject targetPrefab;

    [Header("Shooter Settings")]
    public float targetDistance = 5;
    public float bufferDistance = 2;
    public float fireTime = 10;
    public float targetingTime = 2;

    private float fireTimer;

    private Animator animator;

    void Start()
    {
        base.Start();
        fireTimer = fireTime;
        animator = GetComponentInChildren<Animator>();
    }

    protected override void SetTarget()
    {
    }

    protected override void Wander()
    {
    }

    private void Update()
    {
        base.Update();

        fireTimer -= Time.deltaTime;

        if(fireTimer <= 0)
        {
            StartCoroutine(ShootAtPlayer());
        }
    }

    IEnumerator ShootAtPlayer()
    {
        fireTimer = float.MaxValue;

        animator.SetBool("active", true);

        Vector2 playerPosition = player.position;
        GameObject target = Instantiate(targetPrefab, playerPosition, Quaternion.identity);
        target.GetComponent<Animator>().speed = 1 / targetingTime;

        yield return new WaitForSeconds(targetingTime);

        GameObject bomb = Instantiate(bombPrefab, (Vector3)(playerPosition + Vector2.up * Camera.main.orthographicSize * 2), Quaternion.identity);
        bomb.GetComponent<BombController>().targetHeight = playerPosition.y;

        Destroy(target);

        animator.SetBool("active", false);

        fireTimer = fireTime;
    }
}
