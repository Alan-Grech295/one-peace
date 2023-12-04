using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss3 : EnemyController
{
    public GameObject bombPrefab;
    public GameObject targetPrefab;

    public GameObject bulletPrefab;
    public Transform[] gunPoints;

    [Header("Shooter Settings")]
    public float targetDistance = 5;
    public float bufferDistance = 2;
    public float fireBombTime = 10;
    public float fireBulletsTime = 10;
    public float targetingTime = 2;

    public float spinTime = 10;
    public float spinRate = 10;

    private float fireBombTimer;

    private float fireBulletsTimer;

    private Animator animator;

    void Start()
    {
        fireBombTimer = fireBombTime;
        base.Start();
        animator = GetComponentInChildren<Animator>();
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Enemy"), LayerMask.NameToLayer("Wall"));
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Enemy"), LayerMask.NameToLayer("VisibleWall"));
    }

    protected override void SetTarget()
    {
        aiPath.destination = player.position + (transform.position - player.position).normalized * (targetDistance + bufferDistance);
    }

    protected override void Wander()
    {
        SetTarget();
    }

    private void Update()
    {
        base.Update();

        fireBombTimer -= Time.deltaTime;
        fireBulletsTimer -= Time.deltaTime;

        if (fireBulletsTimer <= 0)
        {
            fireBulletsTimer = fireBulletsTime;

            foreach (Transform t in gunPoints)
            {
                GameObject bullet = Instantiate(bulletPrefab, t.position, t.rotation);
                BulletController bulletController = bullet.GetComponent<BulletController>();
                bulletController.direction = t.up;
                bulletController.parent = transform;
                bulletController.damage = damage;
            }
        }

        if (fireBombTimer <= 0)
        {
            StartCoroutine(ShootAtPlayer());
        }
    }

    IEnumerator ShootAtPlayer()
    {
        fireBombTimer = float.MaxValue;

        //animator.SetBool("active", true);

        Vector2 playerPosition = player.position;
        GameObject target = Instantiate(targetPrefab, playerPosition, Quaternion.identity);
        target.GetComponent<Animator>().speed = 1 / targetingTime;

        yield return new WaitForSeconds(targetingTime);

        GameObject bomb = Instantiate(bombPrefab, (Vector3)(playerPosition + Vector2.up * Camera.main.orthographicSize * 2), Quaternion.identity);
        bomb.GetComponent<BombController>().targetHeight = playerPosition.y;

        Destroy(target);

        //animator.SetBool("active", false);

        fireBombTimer = fireBombTime;
    }
}
