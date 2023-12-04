using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShooterBoss : EnemyController
{
    public GameObject bulletPrefab;
    public Transform[] gunPoints;

    [Header("Shooter Settings")]
    public float targetDistance = 5;
    public float bufferDistance = 2;
    public float fireTime = 0.8f;
    public float spinTime = 10;
    public float spinRate = 10;

    private float fireTimer;
    private float spinTimer;

    private Animator animator;

    void Start()
    {
        base.Start();
        animator = GetComponentInChildren<Animator>();
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

        fireTimer -= Time.deltaTime;
        spinTimer -= Time.deltaTime;

        if (fireTimer <= 0)
        {
            StartCoroutine(Shoot());
        }

        if(spinTimer <= 0)
        {
            StartCoroutine(Spin());
        }
    }

    IEnumerator Spin()
    {
        spinTimer = float.MaxValue;

        float totalRot = 0;
        aiPath.enableRotation = false;
        GetComponent<Collider2D>().isTrigger = true;

        fireTime /= 4f;

        while(totalRot < 720)
        {
            rb.rotation += spinRate * Time.deltaTime;
            totalRot += spinRate * Time.deltaTime;
            yield return null;
        }

        fireTime *= 4f;

        GetComponent<Collider2D>().isTrigger = false;

        aiPath.enableRotation = true;


        spinTimer = spinTime;
    }

    IEnumerator Shoot()
    {
        fireTimer = float.MaxValue;

        foreach (Transform t in gunPoints)
        {
            GameObject bullet = Instantiate(bulletPrefab, t.position, t.rotation);
            BulletController bulletController = bullet.GetComponent<BulletController>();
            bulletController.direction = t.up;
            bulletController.parent = transform;
            bulletController.damage = damage;
            yield return new WaitForSeconds(fireTime / gunPoints.Length);
        }

        fireTimer = fireTime;
    }
}
