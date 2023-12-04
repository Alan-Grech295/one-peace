using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fuel : Consumable
{
    public float fuelAmount = 20;

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
        playerController.AddFuel(fuelAmount);
    }
}
