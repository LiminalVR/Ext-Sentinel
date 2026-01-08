using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WayPoint : MonoBehaviour
{
    [Header("Gizmo Settings")]
    public Color gizmoColor = Color.red;
    public float gizmoSize = 1f;

    // Draw gizmos only when not in play mode
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            Gizmos.color = gizmoColor;
            Gizmos.DrawSphere(transform.position, gizmoSize);
        }
    }
}
