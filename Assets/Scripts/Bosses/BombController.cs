using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombController : MonoBehaviour
{
    public GameObject explosion;
    public float blastRadius = 2;
    public float damage = 50;

    public float targetHeight;
    public float initSpeed;

    Rigidbody2D rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.velocity = Vector2.down * initSpeed;
        transform.rotation = Quaternion.Euler(0, 0, 180);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(rb.position.y <= targetHeight)
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(new Vector2(transform.position.x, targetHeight), blastRadius);

            foreach(Collider2D collider in colliders)
            {
                if (collider.tag != "Player") continue;

                IDamagable damagable = collider.GetComponent<IDamagable>();
                damagable?.Damage(damage, transform);
            }

            Destroy(gameObject);
            Destroy(Instantiate(explosion, new Vector2(transform.position.x, targetHeight), Quaternion.identity), 5);  
        }
    }
}
