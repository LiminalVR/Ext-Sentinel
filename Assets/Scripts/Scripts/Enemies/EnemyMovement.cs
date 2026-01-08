using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    // Public variables for speed and waypoint handling
    public float enemyWaypointSpeed = 3f;
    public float enemyWaypointStoppingDistance = 2f;
    public List<Transform> waypoints = new List<Transform>();

    // Internal tracking
    private int lastNumber;
    private int currentNumber;
    private int wayPointIndex = 0;

    private EnemyHealth enemyHealth;
    public bool isDead = false;

    private void Awake()
    {
        enemyHealth = GetComponent<EnemyHealth>();
    }

    private void OnEnable()
    {
        if (waypoints.Count == 0 && SpawnerManager.Instance != null)
            waypoints = SpawnerManager.Instance.waypoints;

        PickNextWaypoint();
    }

    private void FixedUpdate()
    {
        MoveTowardsWaypoint();
    }

    void MoveTowardsWaypoint()
    {
        if (isDead || waypoints.Count == 0)
            return;

        Transform targetWaypoint = waypoints[wayPointIndex];

        // Move enemy towards waypoint
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetWaypoint.position,
            enemyWaypointSpeed * Time.deltaTime
        );

        // Rotate to face the waypoint
        Vector3 direction = (targetWaypoint.position - transform.position).normalized;
        if (direction != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(direction);

        // Check if reached waypoint
        float distance = Vector3.Distance(transform.position, targetWaypoint.position);
        if (distance <= enemyWaypointStoppingDistance)
        {
            PickNextWaypoint();
        }
    }

    void PickNextWaypoint()
    {
        if (waypoints.Count <= 1) return;
        wayPointIndex = GetRandom(0, waypoints.Count);
    }

    int GetRandom(int min, int max)
    {
        int rand = Random.Range(min, max);
        while (rand == lastNumber && max > 1)
            rand = Random.Range(min, max);

        lastNumber = rand;
        return rand;
    }
}
