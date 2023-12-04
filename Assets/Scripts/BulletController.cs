using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BulletController : MonoBehaviour
{
    public float initialSpeed = 20;
    public float destroyDelay = 10;
    public float damage;
    [HideInInspector] public Vector2 direction;
    [HideInInspector] public Transform parent;
    private Rigidbody2D rb;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.velocity = direction * initialSpeed;
        Destroy(gameObject, destroyDelay);
        foreach(Collider2D col in parent.GetComponents<Collider2D>())
        {
            Physics2D.IgnoreCollision(GetComponent<Collider2D>(), col);
        }

        foreach (Collider2D col in parent.GetComponentsInChildren<Collider2D>())
        {
            Physics2D.IgnoreCollision(GetComponent<Collider2D>(), col);
        }
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = Quaternion.LookRotation(Vector3.forward, direction);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Destroy(gameObject);

        IDamagable damagable = collision.transform.GetComponent<IDamagable>();
        if(damagable != null)
        {
            damagable.Damage(damage, parent);
        }
    }
}
