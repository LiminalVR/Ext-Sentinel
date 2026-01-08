using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LazerShot : MonoBehaviour, IResettableShot
{
    [HideInInspector] public Rigidbody rb;
    public float force = 1;
    public int damage = 1;
    [HideInInspector] public TrailRenderer trailRenderer;
    public float lifeTime = 5f; 
    private float timer;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        trailRenderer = GetComponent<TrailRenderer>();
    }

    void OnEnable()
    {
    timer = 0f;
    rb.isKinematic = false;
    rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    trailRenderer.enabled = true;
    //Debug.Log($"LazerShot enabled at {Time.time}, will live for {lifeTime} seconds");
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= lifeTime)
        {
            //Debug.Log($"LazerShot auto-despawn at {Time.time}, timer: {timer}");
            ResetShot();
        }
        // Optionally, add out-of-bounds check here
    }

    void OnCollisionEnter(Collision collision)
    {
        //Debug.Log($"LazerShot hit {collision.gameObject.name} (tag: {collision.gameObject.tag}) at {Time.time}");

        var enemyHealth = collision.gameObject.GetComponentInParent<EnemyHealth>();
        if (enemyHealth != null)
        {
            enemyHealth.cannonBall = gameObject;
            enemyHealth.TakeDamage(damage);
            ResetShot();
            return;
        }

        var droneHealth = collision.gameObject.GetComponentInParent<DroneHealth>();
        if (droneHealth != null)
        {
            droneHealth.cannonBall = gameObject;
            droneHealth.TakeDamage(damage);
            ResetShot();
            return;
        }

        //Debug.Log("NO EnemyHealth or DroneHealth found on: " + collision.gameObject.name);

        //rb.velocity = rb.velocity / 2;
    }
    public void ResetShot()
    {
    rb.velocity = Vector3.zero;
    rb.angularVelocity = Vector3.zero;
    rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
    rb.isKinematic = true;
    trailRenderer.enabled = false;
    //Debug.Log($"LazerShot ResetShot at {Time.time}");
    gameObject.SetActive(false);
    }
}
