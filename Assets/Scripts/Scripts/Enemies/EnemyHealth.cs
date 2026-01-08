using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyHealth : MonoBehaviour
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

    [Header("Hit Materials")]
    [SerializeField] private Renderer enemyRenderer;  // Assign main Renderer
    [SerializeField] private Material normalMaterial;  // Default material
    [SerializeField] private Material hitMaterial;     // Material to show when hit
    [SerializeField] private float hitFlashDuration = 0.1f; // Duration of hit flash

    [Header("Score UI")]
    [SerializeField] private GameObject scorePopupPrefab;
    [SerializeField] private float scorePopupDuration = 1.5f; // How long the UI stays
    [SerializeField] private Vector3 scorePopupScale = new Vector3(1f, 1f, 1f);
    [SerializeField] private Vector3 scorePopupPopScale = new Vector3(1.5f, 1.5f, 1.5f);



    private Material enemyMaterial;
    private Coroutine hitFlashCoroutine;


    private bool hasSpawnedOnce = false; // ✅ prevents portal at pool init
    private int startHealth;


    private void Start()
    {
        // safer lookup: don't assume a named object exists
        gameManager = GameManager.Instacne;
        if (gameManager == null)
            Debug.LogWarning("[EnemyHealth] GameManager not found in scene. Score/remove calls will be skipped.");

        startHealth = health; // store initial value

        if (enemyRenderer != null && normalMaterial != null)
        {
            enemyRenderer.material = normalMaterial;
        }

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

        enemyMovement = GetComponent<EnemyMovement>();
    }

    private void OnEnable()
    {
        health = startHealth; // Reset health

        if (enemyRenderer != null && normalMaterial != null)
        {
            enemyRenderer.material = normalMaterial;
        }

        if (!hasSpawnedOnce)
        {
            hasSpawnedOnce = true;
            return;
        }

        SpawnPortalEffect();
    }


    private void FlashHit()
    {
        if (enemyRenderer == null || hitMaterial == null || normalMaterial == null) return;

        // Stop previous flash if still running
        if (hitFlashCoroutine != null)
            StopCoroutine(hitFlashCoroutine);

        hitFlashCoroutine = StartCoroutine(HitFlashCoroutine());
    }

    private IEnumerator HitFlashCoroutine()
    {
        // Switch to hit material
        enemyRenderer.material = hitMaterial;

        // Wait for the flash duration
        yield return new WaitForSeconds(hitFlashDuration);

        // Revert to normal material
        enemyRenderer.material = normalMaterial;

        hitFlashCoroutine = null;
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
        //Debug.Log($"{name} hit! Health before: {health}, Damage: {damage}");

        // Trigger material flash
        FlashHit();

        health -= damage;
        //Debug.Log($"{name} health after: {health}");

        if (health <= 0)
        {
            //Debug.Log($"{name} died!");
            Death();
        }
    }

    private void SpawnScorePopup()
    {
        if (scorePopupPrefab == null) return;

        GameObject popup = Instantiate(scorePopupPrefab, transform.position, Quaternion.identity);

        // Set the text
        TMPro.TextMeshProUGUI tmpText = popup.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (tmpText != null)
        {
            tmpText.text = scoreValue.ToString();
        }

        // Optional: Make it front-facing if not using FaceCamera
        popup.transform.LookAt(popup.transform.position + Camera.main.transform.forward);

        // Animate scale
        StartCoroutine(ScorePopupAnimation(popup));

        // Destroy after duration
        Destroy(popup, scorePopupDuration);
    }

    private IEnumerator ScorePopupAnimation(GameObject popup)
    {
        float timer = 0f;
        float animDuration = 0.3f; // pop animation time

        Vector3 startScale = scorePopupScale;
        Vector3 targetScale = scorePopupPopScale;

        while (timer < animDuration)
        {
            timer += Time.deltaTime;
            float t = timer / animDuration;
            popup.transform.localScale = Vector3.Lerp(startScale, targetScale, Mathf.Sin(t * Mathf.PI * 0.5f)); // smooth pop
            yield return null;
        }

        // Lerp back to normal scale
        timer = 0f;
        while (timer < animDuration)
        {
            timer += Time.deltaTime;
            float t = timer / animDuration;
            popup.transform.localScale = Vector3.Lerp(targetScale, startScale, Mathf.Sin(t * Mathf.PI * 0.5f));
            yield return null;
        }

        popup.transform.localScale = startScale;
    }


    public void Death()
    {
        // award score if we have a valid GameManager
        if (gameManager != null)
        {
            gameManager.AddScore(scoreValue);

            if (gameManager.enemies != null)
                gameManager.enemies.Remove(gameObject);
        }
        else
        {
            Debug.LogWarning("[EnemyHealth] Death() called but GameManager reference is null.");
        }

        // mark movement/agent safely
        if (enemyShip)
        {
            if (enemyMovement != null)
                enemyMovement.isDead = true;
            if (agent != null)
                agent.enabled = false;
        }

        if (enemyShoot != null)
            enemyShoot.enabled = false;

        if (enemyShip && enemySpawnerScript != null)
            enemySpawnerScript.enemiesFromThisSpawnerList.Remove(gameObject);

        // Explosion and score UI (SpawnScorePopup is already guarded)
        PlayExplosionEffect();
        SpawnScorePopup();


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
