using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class ShooterController : EnemyController
{
    public GameObject bulletPrefab;
    public AudioClip shootSound;

    [Header("Shooter Settings")]
    public float targetDistance = 5;
    public float bufferDistance = 2;
    public float fireTime = 1;

    private float fireTimer;

    private bool targetingPlayer = false;

    private AudioSource audioSource;

    private void Start()
    {
        base.Start();
        audioSource = GetComponent<AudioSource>();
    }

    protected override void SetTarget()
    {
        if(Vector2.Distance(player.position, transform.position) <= targetDistance ||
           Vector2.Distance(player.position, transform.position) >= targetDistance + bufferDistance)
        {
            aiPath.destination = player.position + (transform.position - player.position).normalized * (targetDistance + bufferDistance);
            targetingPlayer = true;
        }
    }

    protected override void Wander()
    {
        base.Wander();
        targetingPlayer = false;
    }

    private void Update()
    {
        base.Update();

        fireTimer -= Time.deltaTime;
        if (aiPath.reachedEndOfPath && targetingPlayer) 
        {
            Vector2 dirToTarget = player.position - transform.position;
            rb.angularVelocity = 0;
            transform.rotation = Quaternion.Lerp(transform.rotation,
                                 Quaternion.LookRotation(Vector3.forward, dirToTarget), Time.deltaTime * 10);

            if (fireTimer <= 0)
            {
                fireTimer = fireTime;

                GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
                BulletController bulletController = bullet.GetComponent<BulletController>();
                bulletController.direction = transform.up;
                bulletController.parent = transform;
                bulletController.damage = damage;
                audioSource.PlayOneShot(shootSound);
            }
        }
    }
}
