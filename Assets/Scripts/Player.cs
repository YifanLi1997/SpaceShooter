﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    [Header("Player Health")]
    [SerializeField] int health = 1000;
    [SerializeField] GameObject playerExplosionVFXPrefab;
    [SerializeField] float durationOfExplosion = 1f;

    [Header("Player Movement")]
    [SerializeField] float moveSpeed = 20f;
    [SerializeField] float xPadding = 0.5f;
    [SerializeField] float yPadding = 0.5f;

    [Header("Projectile")]
    [SerializeField] GameObject laserPrefab;
    [SerializeField] float projectileSpeed = 50f;
    [SerializeField] float timeBetweenShoot = 0.1f;

    [Header("Sound Effect")]
    [SerializeField] AudioClip deathSFX;
    [SerializeField] AudioClip shootingSFX;
    [SerializeField] [Range(0, 1)] float deathVolume = 1f;
    [SerializeField] [Range(0, 1)] float shootingVolume = 0.25f;

    Camera m_MainCamera;

    float xMin;
    float xMax;
    float yMin;
    float yMax;

    // Start is called before the first frame update
    void Start()
    {
        m_MainCamera = Camera.main;
        SetCameraSpace();
    }

    // Update is called once per frame
    void Update()
    {
        Move();
        Shoot();
    }

    public int GetHealth()
    {
        return health;
    }

    // if we want to extract this health block,
    // we could build another script called Health
    // and apply it to both Enemy and Player.
    // However, the death effects of the two could be very different
    // So there, we will keep it this way
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            DamageDealer damageDealer =
            collision.gameObject.GetComponent<DamageDealer>();
            if (!damageDealer) { return; }
            ProcessHit(damageDealer);
        }

        if (collision.gameObject.CompareTag("Projectile"))
        {
            Projectile projectile =
            collision.gameObject.GetComponent<Projectile>();
            if (!projectile) { return; }
            ProcessHit(projectile);
        }
        
    }

    private void ProcessHit(Projectile projectile)
    {
        health -= projectile.GetDamage();
        projectile.Hit();
        if (health <= 0)
        {
            Die();
        }
    }

    private void ProcessHit(DamageDealer damageDealer)
    {
        health -= damageDealer.GetDamage();
        damageDealer.Hit();
        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // Destory player itself
        Destroy(gameObject);

        // VFX
        GameObject playerExplosion = Instantiate(
            playerExplosionVFXPrefab,
            transform.position,
            transform.rotation);
        Destroy(playerExplosion, durationOfExplosion);

        // SFX
        AudioSource.PlayClipAtPoint(deathSFX, Camera.main.transform.position, deathVolume);

        // Load GameOver Scene
        FindObjectOfType<SceneLoader>().LoadGameOver();
    }

    private void Shoot()
    {
        if(Input.GetButtonDown("Fire1"))
        {
            StartCoroutine(ShootLasers());
        }

        if(Input.GetButtonUp("Fire1"))
        {
            StopAllCoroutines();
        }
    }

    private IEnumerator ShootLasers()
    {
        while (true) { 
            GameObject laser = Instantiate(
                    laserPrefab,
                    transform.position,
                    transform.rotation) as GameObject;
            laser.GetComponent<Rigidbody2D>().velocity = new Vector2(0, projectileSpeed);
            AudioSource.PlayClipAtPoint(shootingSFX, Camera.main.transform.position, shootingVolume);
            yield return new WaitForSeconds(timeBetweenShoot);
        }
    }

    private void Move()
    {
        var xMovement = Input.GetAxis("Horizontal") * moveSpeed * Time.deltaTime;
        var yMovement = Input.GetAxis("Vertical") * moveSpeed * Time.deltaTime;

        var currentPos = transform.position;
        currentPos.x = Mathf.Clamp(currentPos.x + xMovement, xMin, xMax);
        currentPos.y = Mathf.Clamp(currentPos.y + yMovement, yMin, yMax);
        transform.position = currentPos;
    }

    private void SetCameraSpace()
    {
        // due to the problem caused by screen compatibility
        // I manually set the numbers used below
        xMin = m_MainCamera.ViewportToWorldPoint(new Vector3(0.19f, 0, 0)).x;
        xMax = m_MainCamera.ViewportToWorldPoint(new Vector3(0.81f, 0, 0)).x;
        yMin = m_MainCamera.ViewportToWorldPoint(new Vector3(0, 0, 0)).y + 0.75f;
        yMax = m_MainCamera.ViewportToWorldPoint(new Vector3(0, 1, 0)).y - 1f;
    }
}
