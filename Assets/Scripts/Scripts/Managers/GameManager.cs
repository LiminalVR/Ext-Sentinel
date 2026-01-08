using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;
using Liminal.SDK.Core;
using Liminal.Experience;

public class GameManager : MonoBehaviour
{
    [Header("Game Over Effects")]
    public GameObject explosionPrefab;              // Start explosion particle
    public GameObject loopExplosionPrefab;          // Looping explosion particle
    public Transform explosionSpawnPoint;           // Where to spawn explosions
    public AudioClip explosionClip;                 // Explosion sound
    public float explosionVolume = 1f;
    public float resultsDelay = 2f;                 // Delay before fade/transition
    public float loopExplosionDelay = 1f;           // Delay before the loop explosion starts


    [Header("Fade Settings")]
    [Tooltip("Optional: assign a GameObject (Quad/Plane) with Renderer")]
    public GameObject fadeObject;
    [Tooltip("Optional: assign fade material directly (overrides fadeObject)")]
    public Material fadeMaterial;
    [Tooltip("Seconds it takes to fade in/out")]
    public float fadeDuration = 1f;

    private bool isFading = false;


    [Header("Shield Settings")]
    public float currentShield = 100f;
    public float maxShield = 100f;
    public float minShield = 0f;
    public float shieldRegenRate = 2f;

    [Header("Shield Visuals")]
    public Material shieldMaterial;
    public string shieldColorProperty = "_Color";
    public Color hitColor = Color.red;
    public Color normalColor = Color.cyan;
    public float colorFlashDuration = 0.5f;
    public float pulseSpeed = 2f;
    public float pulseIntensity = 0.2f;

    [Header("Shield Audio")]
    public AudioClip shieldHitClip;
    public float shieldHitVolume = 1f;
    private AudioSource audioSource;

    [Header("UI")]
    public Slider shieldSlider;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI waveText;
    public TextMeshProUGUI waveTimerText;

    [Header("Score")]
    public int score = 0;

    [Header("Game Over / Results Settings")]
    public bool gameOverOnShieldBreak = true;
    public string resultsSceneName = "Results"; // set via inspector

    [HideInInspector]
    public System.Collections.Generic.List<GameObject> enemies = new System.Collections.Generic.List<GameObject>();

    [Header("Score Animation Settings")]
    public float popScale = 1.5f;
    public float popDuration = 0.2f;

    [Header("Shield Break Dialogue")]
    public GameObject shieldsBrokenDialogue;
    public float explosionPlayDuration = 3f; // how long explosions play before reset
    public float dialogueDisplayDuration = 4f; // how long the dialogue stays visible


    private Vector3 originalScale;
    private Coroutine flashRoutine;
    private bool isFlashing = false;
    private bool isGameOver = false;

    private SpawnerManager spawnerManager;

    public static GameManager Instacne;

    private void Awake()
    {
        Instacne = this;
    }

    private void Start()
    {
        // ensure spawnerManager reference is assigned
        spawnerManager = SpawnerManager.Instance;

        // If no material assigned manually, get it from fadeObject
        if (fadeMaterial == null && fadeObject != null)
        {
            Renderer r = fadeObject.GetComponent<Renderer>();
            if (r != null)
                fadeMaterial = r.material;
        }

        if (fadeMaterial != null)
        {
            // Start black
            Color color = fadeMaterial.color;
            color.a = 1f;
            fadeMaterial.color = color;

            // Fade into scene
            StartCoroutine(FadeIn());
        }

        if (fadeObject != null)
        {
            fadeMaterial = fadeObject.GetComponent<Renderer>().material;
            Color color = fadeMaterial.color;
            color.a = 0f;
            fadeMaterial.color = color;
        }

        spawnerManager = SpawnerManager.Instance;

        if (shieldSlider != null)
        {
            shieldSlider.minValue = minShield;
            shieldSlider.maxValue = maxShield;
            shieldSlider.value = currentShield;
        }

        if (scoreText != null)
            originalScale = scoreText.transform.localScale;

        UpdateScoreUI();

        if (shieldMaterial != null && shieldMaterial.HasProperty(shieldColorProperty))
            shieldMaterial.SetColor(shieldColorProperty, normalColor);

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
    }

    private void Update()
    {
        if (isGameOver) return;

        if (currentShield < maxShield)
        {
            currentShield += shieldRegenRate * Time.deltaTime;
            currentShield = Mathf.Clamp(currentShield, minShield, maxShield);
            UpdateShieldUI();
        }

        if (!isFlashing && shieldMaterial != null && shieldMaterial.HasProperty(shieldColorProperty))
        {
            float pulse = (Mathf.Sin(Time.time * pulseSpeed) * 0.5f + 0.5f) * pulseIntensity;
            Color pulsedColor = Color.Lerp(normalColor * (1f - pulseIntensity), normalColor, 1f - pulse);
            shieldMaterial.SetColor(shieldColorProperty, pulsedColor);
        }

        if (currentShield <= minShield)
        {
            StartCoroutine(HandleShieldBreak());
        }
    }

    // Replace or patch ModifyShield to early-ignore damage when spawner says shields are invincible
    public void ModifyShield(float amount)
    {
        if (isGameOver) return;

        // If this is damage and the spawner indicates a shield-invincibility window, ignore it
        if (amount < 0f && spawnerManager != null && spawnerManager.IsShieldInvincible())
        {
            Debug.Log("[GameManager] Shield damage ignored due to wave-end invincibility window.");
            return;
        }

        currentShield += amount;
        currentShield = Mathf.Clamp(currentShield, minShield, maxShield);
        UpdateShieldUI();

        if (amount < 0f)
        {
            if (shieldMaterial != null)
            {
                if (flashRoutine != null) StopCoroutine(flashRoutine);
                flashRoutine = StartCoroutine(FlashShieldColor());
            }

            if (shieldHitClip != null && audioSource != null)
            {
                audioSource.PlayOneShot(shieldHitClip, shieldHitVolume);
            }
        }

        if (currentShield <= minShield)
        {
            StartCoroutine(HandleShieldBreak());
        }
    }

    private void UpdateShieldUI()
    {
        if (shieldSlider != null)
            shieldSlider.value = currentShield;
    }

    private void TriggerGameOver()
    {
        if (isGameOver) return;
        isGameOver = true;

        Debug.Log("[GameManager] Game Over Triggered!");

        // Stop waves
        if (spawnerManager != null)
        {
            spawnerManager.StopAllCoroutines();
            spawnerManager.enabled = false;
        }

        // Disable UI
        if (waveText != null) waveText.gameObject.SetActive(false);
        if (waveTimerText != null) waveTimerText.gameObject.SetActive(false);
        if (shieldSlider != null) shieldSlider.gameObject.SetActive(false);

        // 🔥 Play start explosion effect
        if (explosionPrefab != null)
        {
            Transform spawnPoint = explosionSpawnPoint != null ? explosionSpawnPoint : transform;
            Instantiate(explosionPrefab, spawnPoint.position, spawnPoint.rotation);

            // Start coroutine for loop explosion
            if (loopExplosionPrefab != null)
                StartCoroutine(PlayLoopExplosion(spawnPoint));
        }

        // 🔊 Play explosion sound
        if (explosionClip != null && audioSource != null)
        {
            audioSource.PlayOneShot(explosionClip, explosionVolume);
        }

        // ✅ Save score & update high score
        ScoreManager.SubmitScore(score);

    }

    private IEnumerator HandleShieldBreak()
    {
        if (isGameOver) yield break;
        isGameOver = true;

        Debug.Log("[GameManager] Shields Broken! Triggering explosion and reset sequence.");

        // Stop spawning enemies temporarily
        if (spawnerManager != null)
        {
            spawnerManager.StopAllCoroutines();
            spawnerManager.enabled = false;
        }

        // Play explosion
        Transform spawnPoint = explosionSpawnPoint != null ? explosionSpawnPoint : transform;

        GameObject startExplosion = null;
        if (explosionPrefab != null)
            startExplosion = Instantiate(explosionPrefab, spawnPoint.position, spawnPoint.rotation);

        GameObject loopExplosion = null;
        if (loopExplosionPrefab != null)
        {
            yield return new WaitForSeconds(loopExplosionDelay);
            loopExplosion = Instantiate(loopExplosionPrefab, spawnPoint.position, spawnPoint.rotation);
        }

        // Play explosion sound
        if (explosionClip != null && audioSource != null)
            audioSource.PlayOneShot(explosionClip, explosionVolume);

        // Let explosions play for a bit
        yield return new WaitForSeconds(explosionPlayDuration);

        // Stop/cleanup explosions
        if (startExplosion != null) Destroy(startExplosion);
        if (loopExplosion != null) Destroy(loopExplosion);

        // Reset shield to full
        currentShield = maxShield;
        UpdateShieldUI();

        // Restore visuals
        if (shieldMaterial != null && shieldMaterial.HasProperty(shieldColorProperty))
            shieldMaterial.SetColor(shieldColorProperty, normalColor);

        // Activate the dialogue UI
        if (shieldsBrokenDialogue != null)
        {
            shieldsBrokenDialogue.SetActive(true);
            StartCoroutine(DeactivateShieldDialogueAfterDelay());
        }

        // Allow shield regen and enemy spawns again
        if (spawnerManager != null)
            spawnerManager.enabled = true;

        isGameOver = false;

        Debug.Log("[GameManager] Shield restored and dialogue shown.");
    }

    private IEnumerator DeactivateShieldDialogueAfterDelay()
    {
        yield return new WaitForSeconds(dialogueDisplayDuration);

        if (shieldsBrokenDialogue != null)
            shieldsBrokenDialogue.SetActive(false);
    }

    private IEnumerator PlayLoopExplosion(Transform spawnPoint)
    {
        yield return new WaitForSeconds(loopExplosionDelay);

        GameObject loopExplosion = Instantiate(loopExplosionPrefab, spawnPoint.position, spawnPoint.rotation);

        // Optional: make sure it keeps looping if prefab has looping particles
        ParticleSystem ps = loopExplosion.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            var main = ps.main;
            main.loop = true; // Force loop if needed
        }
    }

    private IEnumerator FadeOutAndLoadResults()
    {
        // ⏳ Wait before fading
        if (resultsDelay > 0f)
            yield return new WaitForSeconds(resultsDelay);

        // Fade out
        if (fadeMaterial != null)
            yield return StartCoroutine(FadeOut());

        // Find MyExperienceApp and end experience
        MyExperienceApp.End();
    }

    public void FadeAndLoadResults()
    {
        StartCoroutine(FadeOutAndLoadResults());
    }


    public void AddScore(int amount)
    {
        if (isGameOver) return;

        score += amount;
        UpdateScoreUI();
        StartCoroutine(AnimateScoreText());
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = score.ToString();
    }

    private IEnumerator AnimateScoreText()
    {
        if (scoreText == null) yield break;

        float elapsed = 0f;
        while (elapsed < popDuration)
        {
            float t = elapsed / popDuration;
            scoreText.transform.localScale = Vector3.Lerp(originalScale, originalScale * popScale, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        scoreText.transform.localScale = originalScale * popScale;

        elapsed = 0f;
        while (elapsed < popDuration)
        {
            float t = elapsed / popDuration;
            scoreText.transform.localScale = Vector3.Lerp(originalScale * popScale, originalScale, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        scoreText.transform.localScale = originalScale;
    }

    private IEnumerator FlashShieldColor()
    {
        if (!shieldMaterial.HasProperty(shieldColorProperty)) yield break;

        isFlashing = true;

        float halfDuration = colorFlashDuration / 2f;
        float elapsed = 0f;

        while (elapsed < halfDuration)
        {
            float t = elapsed / halfDuration;
            shieldMaterial.SetColor(shieldColorProperty, Color.Lerp(normalColor, hitColor, t));
            elapsed += Time.deltaTime;
            yield return null;
        }
        shieldMaterial.SetColor(shieldColorProperty, hitColor);

        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            float t = elapsed / halfDuration;
            shieldMaterial.SetColor(shieldColorProperty, Color.Lerp(hitColor, normalColor, t));
            elapsed += Time.deltaTime;
            yield return null;
        }
        shieldMaterial.SetColor(shieldColorProperty, normalColor);

        isFlashing = false;
    }

    private IEnumerator FadeOut()
    {
        if (fadeObject != null)
            fadeObject.SetActive(true); // Ensure it's visible

        if (fadeMaterial != null)
        {
            // Make sure the shader is transparent-capable
            if (fadeMaterial.HasProperty("_Color"))
            {
                float timer = 0f;
                Color color = fadeMaterial.color;
                color.a = 0f; // start clear
                fadeMaterial.color = color;

                // Force render on top (useful if quad is world-space)
                Renderer r = fadeObject != null ? fadeObject.GetComponent<Renderer>() : null;
                if (r != null)
                {
                    r.sortingOrder = 999; // ensures it's always on top
                }

                while (timer < fadeDuration)
                {
                    timer += Time.deltaTime;
                    color.a = Mathf.Lerp(0f, 1f, timer / fadeDuration);
                    fadeMaterial.color = color;
                    yield return null;
                }

                // Fully black at end
                color.a = 1f;
                fadeMaterial.color = color;
            }
            else
            {
                Debug.LogWarning("[GameManager] Fade material has no _Color property!");
            }
        }
        else
        {
            Debug.LogWarning("[GameManager] No fade material assigned!");
        }

        isFading = false;
    }


    private IEnumerator FadeIn()
    {
        if (fadeMaterial != null)
        {
            float timer = 0f;
            Color color = fadeMaterial.color;
            color.a = 1f;

            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                color.a = Mathf.Lerp(1f, 0f, timer / fadeDuration);
                fadeMaterial.color = color;
                yield return null;
            }

            color.a = 0f;
            fadeMaterial.color = color;
        }
        isFading = false;
    }

    private void OnDisable()
    {
        ResetFadeMaterial();
    }

    private void OnApplicationQuit()
    {
        ResetFadeMaterial();
    }

    private void ResetFadeMaterial()
    {
        if (fadeMaterial != null && fadeMaterial.HasProperty("_Color"))
        {
            Color c = fadeMaterial.color;
            c.a = 0f; // fully transparent
            fadeMaterial.color = c;
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(fadeMaterial); // ensures editor saves this reset
#endif
            Debug.Log("[GameManager] Reset fade material to alpha=0");
        }
    }


#if UNITY_EDITOR
    private void OnValidate()
    {
        ResetFadeMaterial();
    }
#endif


}
