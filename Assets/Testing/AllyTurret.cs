using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AllyTurret : MonoBehaviour
{
    [Header("Shooting Settings")]
    public GameObject cannonBall;        
    public Transform[] firePoints;       // Only need firepoints now
    public float fireRate = 2f;
    public float targetingRange = 100f;
    public float minFireRate = 1f;
    public float maxFireRate = 3f;
    private float myFireRate;
    private float aimTimer = 0f;
    public float requiredAimTime = 1.5f;
    

    [Header("Targeting")]
    private Transform currentTarget;
    public Transform turretBase; // Assign this in the Inspector
    public Transform turretCannonBarrel; // Assign this in the Inspector

    
    [Header("Recoil Settings")]
    public float recoilDistance = 0.5f;
    public float recoilReturnSpeed = 8f;
    private Vector3 cannonOriginalLocalPos;
    private bool isRecoiling = false;
    private float fireTimer;
    private ParticleSystem[] muzzleFlashes;

    [Header("Audio")]
    [SerializeField] private AudioClip explosionSound;
    [SerializeField, Range(0f, 1f)] private float explosionVolume = 1f;
    [SerializeField] private bool explosionSound2D = false;
    [SerializeField] private float explosionMinDistance = 1f;
    [SerializeField] private float explosionMaxDistance = 50f;

    void Start()
    {
        // Auto-grab muzzle flashes from firepoints
        muzzleFlashes = new ParticleSystem[firePoints.Length];
        for (int i = 0; i < firePoints.Length; i++)
        {
            if (firePoints[i] != null)
                muzzleFlashes[i] = firePoints[i].GetComponentInChildren<ParticleSystem>();
        }

        // Store original local position for recoil
        if (turretCannonBarrel != null)
            cannonOriginalLocalPos = turretCannonBarrel.localPosition;

        myFireRate = Random.Range(minFireRate, maxFireRate);
    }

    void Update()
    {
        if (currentTarget == null || !currentTarget.gameObject.activeInHierarchy)
            currentTarget = FindRandomEnemy();

        if (currentTarget == null) return;

        // Aiming logic
        Vector3 dir = (currentTarget.position - transform.position).normalized;
        Quaternion lookRot = Quaternion.LookRotation(dir);

        if (turretBase != null)
            turretBase.rotation = Quaternion.Slerp(turretBase.rotation, lookRot, Time.deltaTime * 2f);

        float angle = Quaternion.Angle(turretBase.rotation, lookRot);
        if (angle < 10f) // within 10 degrees
            aimTimer += Time.deltaTime;
        else
            aimTimer = 0f;

        fireTimer += Time.deltaTime;
        if (fireTimer >= myFireRate && aimTimer >= requiredAimTime)
        {
            FireCannonballs();
            fireTimer = 0f;
            aimTimer = 0f;
            myFireRate = Random.Range(minFireRate, maxFireRate); // randomize for next shot
        }

        // Recoil return logic
        if (turretCannonBarrel != null)
        {
            if (isRecoiling)
            {
                // Lerp back to original position
                turretCannonBarrel.localPosition = Vector3.Lerp(turretCannonBarrel.localPosition, cannonOriginalLocalPos, Time.deltaTime * recoilReturnSpeed);
                // Stop lerping if close enough
                if (Vector3.Distance(turretCannonBarrel.localPosition, cannonOriginalLocalPos) < 0.01f)
                {
                    turretCannonBarrel.localPosition = cannonOriginalLocalPos;
                    isRecoiling = false;
                }
            }
        }
    }

    private void FireCannonballs()
    {
        for (int i = 0; i < firePoints.Length; i++)
        {
            Transform firePoint = firePoints[i];
            if (firePoint == null) continue;

            GameObject pooledBolt = PoolManager.current.GetPooledObject(cannonBall.name);
            if (pooledBolt == null) return;

            // Try CannonBall logic first
            var cb = pooledBolt.GetComponent<CannonBall>();
            if (cb != null)
            {
                cb.firedFrom = null;
                cb.rb.transform.position = firePoint.position;
                cb.rb.transform.rotation = firePoint.rotation;
                pooledBolt.SetActive(true);
                cb.rb.isKinematic = false;
                //cb.trailRenderer.Clear();
                //cb.trailRenderer.enabled = true;
                cb.rb.AddForce(cb.rb.transform.forward * cb.force, ForceMode.Impulse);
                
                PlayAllyShootingAudio();
            }
            else
            {
                // Try LazerShot logic
                var lb = pooledBolt.GetComponent<LazerShot>();
                if (lb != null)
                {
                    lb.rb.transform.position = firePoint.position;
                    lb.rb.transform.rotation = firePoint.rotation;
                    pooledBolt.SetActive(true);
                    lb.rb.isKinematic = false;
                    lb.trailRenderer.Clear();
                    lb.trailRenderer.enabled = true;
                    lb.rb.AddForce(lb.rb.transform.forward * lb.force, ForceMode.Impulse);
                    // Add any LazerShot-specific effects here if needed
                    PlayAllyShootingAudio();
                }
            }

            // 🎇 Play muzzle flash if it exists
            if (muzzleFlashes[i] != null)
            {
                var main = muzzleFlashes[i].main;
                main.startRotation = Random.Range(0f, Mathf.PI * 2f);
                muzzleFlashes[i].Play();
            }
        }

        // Trigger recoil
        if (turretCannonBarrel != null)
        {
            turretCannonBarrel.localPosition = cannonOriginalLocalPos - Vector3.forward * recoilDistance;
            isRecoiling = true;
        }
    }

    private Transform FindRandomEnemy()
    {
        if (GameManager.Instacne.enemies.Count == 0) return null;
        return GameManager.Instacne.enemies[Random.Range(0, GameManager.Instacne.enemies.Count)].transform;
    }

        // Option B: pick closest enemy (if you want smarter allies)
        /*
        Transform closest = null;
        float minDist = Mathf.Infinity;
        foreach (var e in enemies)
        {
            float dist = Vector3.Distance(transform.position, e.transform.position);
            if (dist < minDist && dist <= targetingRange)
            {
                minDist = dist;
                closest = e.transform;
            }
        }
        return closest;
    }
        */

    private void PlayAllyShootingAudio()
    {
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
