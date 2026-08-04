using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class EnemyShip : MonoBehaviour
{
    // For flying
    public float flySpeed = 15f;
    public Vector3 direction = Vector3.left; // Keep moving forwards! (Relatively, I mean.)

    // For shooting bullets
    public GameObject bulletPrefab;
    public float fireRate = 6f;
    private float fireTimer = 0f;
    public Transform fireTarget;
    public float fireAccuracy = 0.1f; // The higher this number is, the more stormtrooper-like the accuracy will be.
    public float bulletSpeed = 20f;

    // For it's demise
    bool exploding = false;
    float explosionTimer = 0f;
    public float explosionTime = 3f;

    void Update()
    {
        transform.Translate(direction * flySpeed * Time.deltaTime, Space.World);

        // Have we been hit yet?
        if (exploding)
        {
            explosionTimer += Time.deltaTime;
            if (explosionTimer >= explosionTime)
            {
                // Explode.
                Destroy(gameObject);
            }
        }
        else
        {
            // If it's time to shoot, then let's shoot!
            fireTimer += Time.deltaTime;
            if (fireTimer >= fireRate)
            {
                fireTimer = 0f; // Reset the timer so we can start anew.
                Fire();
            }
        }
    }

    void Fire()
    {
        // Shoots a bullet in the general direction of the player.

        Vector3 dir = (fireTarget.position - transform.position).normalized;

        // Randomise the exact position we're firing at.
        dir.x += Random.Range(-fireAccuracy, fireAccuracy);
        dir.y += Random.Range(-fireAccuracy, fireAccuracy);
        dir.z += Random.Range(-fireAccuracy, fireAccuracy);
        dir.Normalize();

        // And... fire!!
        GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        rb.linearVelocity = dir * bulletSpeed; // Determine the velocity.
    }

    void OnTriggerEnter(Collider other)
    {
        // Check if we've hit one of those darn invisible walls
        if (other.CompareTag("Border"))
        {
            // Wrong way, turn back!
            direction = -direction; // Moving direction
            transform.Rotate(0, 180, 0); // Facing direction
        }
    }

    void Death()
    {
        // Begin the chain of events that causes us to become death.
        exploding = true;
    }
}
