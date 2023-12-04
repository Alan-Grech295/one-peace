using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ammo : Consumable
{
    public int ammoAmount = 3;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    protected override void OnPickUp(PlayerController playerController)
    {
        playerController.AddAmmo(ammoAmount);
    }
}
