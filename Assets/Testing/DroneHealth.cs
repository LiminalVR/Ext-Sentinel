using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DroneHealth : MonoBehaviour
{
    [HideInInspector] public NavMeshAgent agent;
    public int health = 1;
    private EnemyShoot enemyShoot;
    [HideInInspector] public SpawnerManager enemySpawnerScript;
    [HideInInspector] public BoxCollider boxCollider;
    private GameManager gameManager;
    private EnemyMovement enemyMovement;
    public bool bossShip;
    public bool enemyShip;
    public bool isDrone;
    [HideInInspector] public GameObject cannonBall;

    [Header("Scoring")]
    public int scoreValue = 100;

    [Header("Shield Damage")]
    public float shieldDamage = 10f;

    [Header("Effects")]
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private float explosionLifetime = 3f;

    [Header("Portal Spawn Effects")]
    [SerializeField] private GameObject portalPrefab;
    [SerializeField] private float portalLifetime = 3f;

    [Header("Audio")]
    [SerializeField] private AudioClip explosionSound;
    [SerializeField, Range(0f, 1f)] private float explosionVolume = 1f;
    [SerializeField] private bool explosionSound2D = false;
    [SerializeField] private float explosionMinDistance = 1f;
    [SerializeField] private float explosionMaxDistance = 50f;

    [Header("Portal Audio")]
    [SerializeField] private AudioClip portalSound;
    [SerializeField, Range(0f, 1f)] private float portalVolume = 1f;
    [SerializeField] private bool portalSound2D = false;
    [SerializeField] private float portalMinDistance = 1f;
    [SerializeField] private float portalMaxDistance = 50f;

    private bool hasSpawnedOnce = false; // ✅ prevents portal at pool init

    void Awake()
    {
        //Debug.Log("DroneHealth Awake on: " + gameObject.name);

        if (enemyShip)
        {
            boxCollider = GetComponentInChildren<BoxCollider>();
            agent = GetComponentInChildren<NavMeshAgent>();
            enemyShoot = GetComponentInChildren<EnemyShoot>();
        }

        if (bossShip)
        {
            boxCollider = GetComponent<BoxCollider>();
            enemyShoot = GetComponent<EnemyShoot>();
        }
    }

    private void Start()
    {
        gameManager = GameManager.Instacne;
        enemyMovement = GetComponent<EnemyMovement>();
        //Debug.Log("Current health: " + health);
    }

    private void OnEnable()
    {
        // Skip very first activation (pool setup)
        if (!hasSpawnedOnce)
        {
            hasSpawnedOnce = true;
            return;
        }

        // ✅ Only runs when spawned into play
        SpawnPortalEffect();
    }

    private void SpawnPortalEffect()
    {
        if (portalPrefab != null)
        {
            GameObject portal = Instantiate(portalPrefab, transform.position, Quaternion.identity);
            if (portalLifetime > 0f)
            {
                Destroy(portal, portalLifetime);
            }
        }

        if (portalSound != null)
        {
            GameObject audioGO = new GameObject("PortalAudio");
            audioGO.transform.position = transform.position;
            var src = audioGO.AddComponent<AudioSource>();

            src.spatialBlend = portalSound2D ? 0f : 1f;
            src.rolloffMode = AudioRolloffMode.Linear;
            src.minDistance = portalMinDistance;
            src.maxDistance = Mathf.Max(portalMaxDistance, portalMinDistance + 0.01f);
            src.playOnAwake = false;

            src.PlayOneShot(portalSound, portalVolume);
            Destroy(audioGO, portalSound.length + 0.1f);
        }
    }

    void Update()
    {
        if (transform.position.y <= -12f)
        {
            if (enemyShip && enemySpawnerScript != null)
                enemySpawnerScript.enemiesFromThisSpawnerList.Remove(gameObject);

            if (gameManager != null)
            {
                //Debug.Log("[EnemyHealth] Enemy fell out of world, reducing shield by " + shieldDamage);
                gameManager.ModifyShield(-shieldDamage);

                // ✅ Play explosion when shield is damaged
                PlayExplosionEffect();
            }

            gameObject.SetActive(false);
        }
    }

    public void TakeDamage(int damage)
    {
        //Debug.Log("TakeDamage called on: " + gameObject.name + " for " + damage);
        health -= damage;
        //Debug.Log("Took damage, current health: " + health);
        if (health <= 0)
        {
            Death();
        }
    }


    public void Death()
    {
        if (scoreValue > 0 && gameManager != null)
        {
            gameManager.AddScore(scoreValue);
            // show popup, etc.
        }

        if (gameManager != null)
        {
            gameManager.AddScore(scoreValue);
            gameManager.enemies.Remove(gameObject);
        }

        if (enemyShip)
        {
            if (enemyMovement != null) enemyMovement.isDead = true;
            if (agent != null) agent.enabled = false;
        }

        if (enemyShoot != null)
            enemyShoot.enabled = false;

        if (enemyShip && enemySpawnerScript != null)
            enemySpawnerScript.enemiesFromThisSpawnerList.Remove(gameObject);

        // Drone-specific logic
        if (isDrone)
        {
            // Add drone death effects here if needed
        }

        // ✅ Explosion on death
        PlayExplosionEffect();

        gameObject.SetActive(false);
    }

    /// <summary>
    /// Handles instantiating explosion VFX + SFX
    /// </summary>
    private void PlayExplosionEffect()
    {
        if (explosionPrefab != null)
        {
            GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            if (explosionLifetime > 0f)
            {
                Destroy(explosion, explosionLifetime);
            }
        }

        if (explosionSound != null)
        {
            GameObject audioGO = new GameObject("ExplosionAudio");
            audioGO.transform.position = transform.position;
            var src = audioGO.AddComponent<AudioSource>();

            src.spatialBlend = explosionSound2D ? 0f : 1f;
            src.rolloffMode = AudioRolloffMode.Linear;
            src.minDistance = explosionMinDistance;
            src.maxDistance = Mathf.Max(explosionMaxDistance, explosionMinDistance + 0.01f);
            src.playOnAwake = false;

            src.PlayOneShot(explosionSound, explosionVolume);
            Destroy(audioGO, explosionSound.length + 0.1f);
        }
    }
}
