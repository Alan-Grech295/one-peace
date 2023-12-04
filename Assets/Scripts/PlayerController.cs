using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour, IDamagable
{
    public GameObject explosion;
    public float moveSpeed = 2f;
    public float rotateSpeed;
    public float damage = 50;
    public float hitDamage = 10;
    public int maxAmmo = 20;
    public float fuelUseRate = 1;
    public float fireTime;
    public ParticleSystem muzzleFlash;
    public GameObject gameOverScreen;

    public int score { get; private set; }

    public GameObject bulletPrefab;

    public Slider healthSlider;
    public Image fuelGauge;
    public Image fuelSlider;
    public Vector2 fuelRotRange;
    public Image ammoSlider;

    public AudioClip idleAudio;
    public AudioClip movingAudio;
    public AudioClip fireAudio;

    private Rigidbody2D rb;

    public float health { get; private set; } = 100;

    public float fuel { get; private set; } = 100;
    public int ammo { get; private set; }
    private float fireTimer;

    private bool dead = false;

    private AudioSource[] audioSources;
    private AudioSource fireSource;
    private AudioSource movingSource;
    
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        healthSlider.value = health;
        ammo = maxAmmo;
        fuel = 100;
        ammoSlider.fillAmount = 1;

        fuelGauge.rectTransform.rotation = Quaternion.Euler(0, 0, fuelRotRange.x);
        fuelSlider.fillAmount = fuel / 100f;
        gameOverScreen.SetActive(false);

        audioSources = GetComponents<AudioSource>();
        audioSources[0].clip = idleAudio;
        audioSources[0].loop = true;
        audioSources[0].Play();

        audioSources[1].clip = movingAudio;
        audioSources[1].playOnAwake = false;
        audioSources[1].loop = true;
        movingSource = audioSources[1];

        audioSources[2].clip = fireAudio;
        audioSources[2].playOnAwake = false;
        audioSources[2].loop = false;
        fireSource = audioSources[2];
    }

    void Update()
    {
        if (dead) return;

        fireTimer -= Time.deltaTime;

        Vector2 movement = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        transform.Rotate(0, 0, -movement.x * rotateSpeed * Time.deltaTime);
        rb.velocity = transform.up * moveSpeed * movement.y;

        if (movement != Vector2.zero)
        {
            if(!movingSource.isPlaying)
                movingSource.Play();

            fuel -= fuelUseRate * Time.deltaTime;
            fuelGauge.rectTransform.rotation = Quaternion.Euler(0, 0, Mathf.Lerp(fuelRotRange.y, fuelRotRange.x, fuel / 100f));
            fuelSlider.fillAmount = fuel / 100f;
            if (fuel <= 0)
            {
                fuel = 0;
                Die();
            }
        }
        else
        {
            if (movingSource.isPlaying)
                movingSource.Stop();
        }

        if (Input.GetKey(KeyCode.Space) && fireTimer <= 0 && ammo > 0)
        {
            fireTimer = fireTime;
            GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
            BulletController bulletController = bullet.GetComponent<BulletController>();
            bulletController.direction = transform.up;
            bulletController.parent = transform;
            bulletController.damage = damage;
            ammo--;
            ammoSlider.fillAmount = ammo / (float)maxAmmo;
            muzzleFlash.Play();
            fireSource.PlayOneShot(fireAudio);
        }
    }

    public void Damage(float damage, Transform damager)
    {
        health -= damage;

        healthSlider.value = health;
        if (health <= 0)
            Die();
    }

    public void Die()
    {
        if (dead) return;

        dead = true;
        rb.bodyType = RigidbodyType2D.Static;
        Destroy(Instantiate(explosion, transform.position, Quaternion.identity), 3);
        gameOverScreen.SetActive(true);
    }

    public void AddScore(int points)
    {
        score += points;
    }

    public void AddFuel(float fuel)
    {
        this.fuel += fuel;
        this.fuel = Mathf.Clamp(this.fuel, 0, 100);
        fuelGauge.rectTransform.rotation = Quaternion.Euler(0, 0, Mathf.Lerp(fuelRotRange.y, fuelRotRange.x, this.fuel / 100f));
        fuelSlider.fillAmount = fuel / 100f;
    }
    
    public void AddHealth(float health)
    {
        this.health += health;
        this.health = Mathf.Clamp(this.health, 0, 100);
        healthSlider.value = this.health;
    }

    public void AddAmmo(int ammo)
    {
        this.ammo += ammo;
        this.ammo = Mathf.Clamp(this.ammo, 0, maxAmmo);
        ammoSlider.fillAmount = this.ammo / (float)maxAmmo;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.transform.tag == "Enemy")
        {
            collision.transform.GetComponent<IDamagable>().Damage(hitDamage, transform);
        }
    }
}