using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEngine;

public class PickupSpawner : MonoBehaviour
{
    public PlayerController player;

    public float checkDistance = 30;

    public GameObject healthConsumable;
    public GameObject fuelConsumable;
    public GameObject ammoConsumable;

    public int maxPickups = 10;

    public float healthGenerateThreshold;
    public float fuelGenerateThreshold;
    public int ammoGenerateThreshold;

    private float calcHealth;
    private float calcFuel;
    private float calcAmmo;

    private List<Consumable> healthPickups = new List<Consumable>();
    private List<Consumable> fuelPickups = new List<Consumable>();
    private List<Consumable> ammoPickups = new List<Consumable>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        calcHealth = healthPickups.Where(pickup => Vector2.Distance(pickup.transform.position, player.transform.position) <= checkDistance)
                                  .Select(pickup => ((Health)pickup).healthAmount)
                                  .Sum() + player.health;

        calcFuel = fuelPickups.Where(pickup => Vector2.Distance(pickup.transform.position, player.transform.position) <= checkDistance)
                                  .Select(pickup => ((Fuel)pickup).fuelAmount)
                                  .Sum() + player.fuel;

        calcAmmo = ammoPickups.Where(pickup => Vector2.Distance(pickup.transform.position, player.transform.position) <= checkDistance)
                                  .Select(pickup => ((Ammo)pickup).ammoAmount)
                                  .Sum() + player.ammo;

        if (healthPickups.Count < maxPickups && calcHealth <= healthGenerateThreshold)
        {
            GameObject consumableGo = Instantiate(healthConsumable, GetSpawnPoint(), Quaternion.identity);
            Consumable consumable = consumableGo.GetComponent<Consumable>();
            consumable.OnPickUpEvent += (item) =>
            {
                healthPickups.Remove(item);
            };
            healthPickups.Add(consumable);
        }

        if (fuelPickups.Count < maxPickups && calcFuel <= fuelGenerateThreshold)
        {
            GameObject consumableGo = Instantiate(fuelConsumable, GetSpawnPoint(), Quaternion.identity);
            Consumable consumable = consumableGo.GetComponent<Consumable>();
            consumable.OnPickUpEvent += (item) =>
            {
                fuelPickups.Remove(item);
            };
            fuelPickups.Add(consumable);
        }

        if (ammoPickups.Count < maxPickups &&calcAmmo <= ammoGenerateThreshold)
        {
            GameObject consumableGo = Instantiate(ammoConsumable, GetSpawnPoint(), Quaternion.identity);
            Consumable consumable = consumableGo.GetComponent<Consumable>();
            consumable.OnPickUpEvent += (item) =>
            {
                ammoPickups.Remove(item);
            };
            ammoPickups.Add(consumable);
        }
    }

    Vector3 GetSpawnPoint()
    {
        Vector2 playerPos = player.transform.position;
        Vector2 spawnPoint = Utils.SpawnInCircle(playerPos, 0, checkDistance);

        for (int i = 0; i < 50 && !GameManager.Instance.mapBounds.Contains(spawnPoint); i++)
        {
            spawnPoint = Utils.SpawnInCircle(playerPos, 0, checkDistance);
        }

        return spawnPoint;
    }

    bool VisibleByCamera(Vector2 point)
    {
        Vector3 viewportPoint = Camera.main.WorldToViewportPoint(point);
        return (new Rect(0, 0, 1, 1)).Contains(viewportPoint);
    }
}
