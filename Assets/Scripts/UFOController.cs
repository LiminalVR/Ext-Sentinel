using System.Collections;
using UnityEngine;

public class UFOController : MonoBehaviour
{
    [Header("Movement Settings")]
    public Transform startPosition;        // Where the UFO begins (off-screen, etc.)
    public float moveDuration = 3f;        // Duration to move into initial position

    [Header("End Game Movement")]
    public Transform endGamePosition;      // Where the UFO moves after all waves complete
    public float endMoveDuration = 4f;     // Duration for end-game movement

    [Header("References")]
    public SpawnerManager spawnerManager;  // Assign in Inspector or auto-find

    private Vector3 finalPosition;         // Position where it sits during gameplay
    private bool isMoving = false;
    private Transform[] childObjects;

    private void Start()
    {
        // Save editor-placed position as the "final" in-game position
        finalPosition = transform.position;

        // Cache children
        childObjects = GetComponentsInChildren<Transform>(includeInactive: true);

        // Hide children at start
        SetChildrenActive(false);

        // Auto-find SpawnerManager if not set
        if (spawnerManager == null)
        {
            spawnerManager = SpawnerManager.Instance;
        }

        // Subscribe to both wave start and completion events
        if (spawnerManager != null)
        {
            spawnerManager.BeginSpawningEvent += OnWavesBegin;
            spawnerManager.OnAllWavesComplete += OnAllWavesComplete;
        }
    }

    private void OnDestroy()
    {
        if (spawnerManager != null)
        {
            spawnerManager.BeginSpawningEvent -= OnWavesBegin;
            spawnerManager.OnAllWavesComplete -= OnAllWavesComplete;
        }
    }

    private void OnWavesBegin()
    {
        if (!isMoving && startPosition != null)
        {
            transform.position = startPosition.position;
            SetChildrenActive(true);
            StartCoroutine(MoveToPosition(startPosition.position, finalPosition, moveDuration));
        }
    }

    private void OnAllWavesComplete()
    {
        if (endGamePosition != null && !isMoving)
        {
            StartCoroutine(MoveToPosition(transform.position, endGamePosition.position, endMoveDuration));
        }
    }

    private IEnumerator MoveToPosition(Vector3 from, Vector3 to, float duration)
    {
        isMoving = true;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            t = Mathf.SmoothStep(0f, 1f, t);
            transform.position = Vector3.Lerp(from, to, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = to;
        isMoving = false;
    }

    private void SetChildrenActive(bool state)
    {
        foreach (Transform child in childObjects)
        {
            if (child != this.transform)
                child.gameObject.SetActive(state);
        }
    }
}
