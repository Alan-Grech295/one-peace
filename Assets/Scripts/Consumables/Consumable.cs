using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Consumable : MonoBehaviour
{
    public delegate void OnPickUpItem(Consumable item);
    public event OnPickUpItem OnPickUpEvent;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Player")
        {
            OnPickUp(collision.transform.GetComponent<PlayerController>());
            OnPickUpEvent?.Invoke(this);
            Destroy(gameObject);
        }
    }

    protected abstract void OnPickUp(PlayerController playerController);
}
