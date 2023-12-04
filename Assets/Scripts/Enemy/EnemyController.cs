using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class EnemyController : MonoBehaviour, IDamagable
{
    public Transform player;
    public GameObject explosion;
    public float chaseSpeed = 3;
    public float wanderSpeed = 2;
    public float seeDistance = 10;
    public float shotSeeMultiplier = 2;
    public float wanderDistance = 5;
    public float wanderDistanceTowardsPlayer = 2;
    public float damage = 20;
    public int numPoints = 10;
    public float maxHealth = 100;

    public GameObject healthBar;
    public delegate void OnDeathEvent(GameObject enemy);
    public event OnDeathEvent OnDeath;

    [Header("AI Settings")]
    public float updateDistance = 0.5f;
    protected AIPath aiPath;
    protected Rigidbody2D rb;

    [Header("Other")]
    public float healthBarHeight = 1f;

    protected Vector2 pastTargetPos;
    protected float health = 100;
    protected Slider healthSlider;

    protected const float graphUpdateDist = 1;
    protected Vector2 pastPos;
    protected Collider2D collider;

    protected Animator animator;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        aiPath = GetComponent<AIPath>();
        rb = GetComponent<Rigidbody2D>();
        collider = GetComponent<Collider2D>();
        animator = GetComponentInChildren<Animator>();

        pastTargetPos = Vector2.positiveInfinity;
        pastPos = Vector2.positiveInfinity;

        // Creating a health bar
        healthBar = Instantiate(healthBar);
        health = maxHealth;
        healthSlider = healthBar.GetComponentInChildren<Slider>();
        healthSlider.maxValue = maxHealth;

        healthSlider.value = health;
    }

    // Update is called once per frame
    protected void Update()
    {
        healthBar.transform.position = transform.position + Vector3.up * healthBarHeight;

        if (Vector2.Distance(transform.position, pastPos) > graphUpdateDist)
        {
            pastPos = transform.position;
            //Bounds bounds = collider.bounds;
            //bounds.center = transform.position;
            //bounds.Expand(seeDistance);
            //AstarPath.active.UpdateGraphs(bounds);
        }

        if (Vector2.Distance(transform.position, player.position) <= seeDistance)
        {
            if (Vector2.Distance(pastTargetPos, player.position) >= updateDistance)
            {
                aiPath.maxSpeed = chaseSpeed;
                pastTargetPos = player.position;
                SetTarget();
            }
        }
        else
        {
            Wander();
        }
    }

    protected abstract void SetTarget();

    protected virtual void Wander()
    {
        aiPath.maxSpeed = wanderSpeed;
        if(aiPath.reachedEndOfPath || float.IsInfinity(aiPath.destination.x))
        {
            aiPath.destination = Random.insideUnitSphere * wanderDistance + transform.position + (player.transform.position - transform.position).normalized * wanderDistanceTowardsPlayer;
        }
    }

    public void Damage(float damage, Transform damager)
    {
        health -= damage;
        healthSlider.value = health;

        if (damager == player.transform && Vector2.Distance(transform.position, damager.position) <= seeDistance * shotSeeMultiplier)
        {
            aiPath.maxSpeed = chaseSpeed;
            pastTargetPos = player.position;
            SetTarget();
        }

        if (health <= 0)
            Die();
    }

    public virtual void Die()
    {
        Destroy(gameObject);
        Destroy(healthSlider.transform.parent.gameObject);
        Destroy(Instantiate(explosion, transform.position, Quaternion.identity), 3);

        player.GetComponent<PlayerController>().AddScore(numPoints);
        OnDeath?.Invoke(gameObject);
    }
}
