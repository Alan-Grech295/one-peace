using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BomberController : EnemyController
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.transform.tag == "Player")
        {
            IDamagable damagable = collision.transform.GetComponent<IDamagable>();
            damagable.Damage(damage, transform);
            Die();
        }
    }
    protected override void SetTarget()
    {
        aiPath.destination = player.position;
    }
}
