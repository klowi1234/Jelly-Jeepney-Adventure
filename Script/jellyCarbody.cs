using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider2D))]
public class SoftBody2D : MonoBehaviour
{
    [Header("Soft Body Points")]
    public Transform[] points; // assign point nodes (Rigidbody2D on each)

    [Header("Soft Body Settings")]
    public float shapeMatchStrength = 0.2f;
    public float pressure = 1.5f;
    public float damping = 3f;

    // cached refs
    private Rigidbody2D[] rbs;
    private Vector2[] originalOffsets;

    void Start()
    {
        if (points == null || points.Length == 0)
        {
            Debug.LogWarning("SoftBody2D: no points assigned.");
            return;
        }

        rbs = new Rigidbody2D[points.Length];
        originalOffsets = new Vector2[points.Length];
        Vector2 c = GetCenter();

        for (int i = 0; i < points.Length; i++)
        {
            var rb = points[i].GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                Debug.LogError($"SoftBody2D: point {points[i].name} has no Rigidbody2D.");
            }
            rbs[i] = rb;
            originalOffsets[i] = (Vector2)points[i].position - c;
        }
    }

    void FixedUpdate()
    {
        if (rbs == null || rbs.Length == 0) return;

        Vector2 center = GetCenter();
        Vector2 averageVel = GetAverageLinearVelocity();

        ApplyShapeMatching(center);
        ApplyPressure(center);
        ApplyDamping(averageVel);
    }

    Vector2 GetCenter()
    {
        Vector2 sum = Vector2.zero;
        foreach (Transform p in points) sum += (Vector2)p.position;
        return sum / points.Length;
    }

    Vector2 GetAverageLinearVelocity()
    {
        Vector2 sum = Vector2.zero;
        int count = 0;
        foreach (var rb in rbs)
        {
            if (rb == null) continue;
            sum += rb.linearVelocity;
            count++;
        }
        return count > 0 ? sum / count : Vector2.zero;
    }

    void ApplyShapeMatching(Vector2 center)
    {
        // compute an average rotation (in degrees) needed to best match the original offsets
        float averageAngle = 0f;
        for (int i = 0; i < points.Length; i++)
        {
            var p = points[i];
            if (p == null) continue;
            Vector2 currentDir = (Vector2)p.position - center;
            Vector2 originalDir = originalOffsets[i];
            // if one is near zero skip to avoid NaN
            if (currentDir.sqrMagnitude < 1e-6f || originalDir.sqrMagnitude < 1e-6f) continue;
            float angle = Vector2.SignedAngle(originalDir, currentDir);
            averageAngle += angle;
        }
        averageAngle /= points.Length;

        for (int i = 0; i < points.Length; i++)
        {
            var p = points[i];
            var rb = rbs[i];
            if (p == null || rb == null) continue;

            Vector2 target = center + Rotate(originalOffsets[i], averageAngle);
            Vector2 diff = target - (Vector2)p.position;
            rb.AddForce(diff * shapeMatchStrength, ForceMode2D.Force);
        }
    }

    void ApplyPressure(Vector2 center)
    {
        // Simple radial pressure away from the center proportional to average distance
        float averageDistance = 0f;
        for (int i = 0; i < points.Length; i++)
            averageDistance += Vector2.Distance(center, points[i].position);
        averageDistance /= points.Length;

        for (int i = 0; i < points.Length; i++)
        {
            var p = points[i];
            var rb = rbs[i];
            if (p == null || rb == null) continue;

            Vector2 dir = ((Vector2)p.position - center);
            if (dir.sqrMagnitude < 1e-6f) continue;
            dir.Normalize();
            rb.AddForce(dir * pressure * averageDistance, ForceMode2D.Force);
        }
    }

    void ApplyDamping(Vector2 averageVel)
    {
        for (int i = 0; i < rbs.Length; i++)
        {
            var rb = rbs[i];
            if (rb == null) continue;
            Vector2 relative = rb.linearVelocity - averageVel;
            rb.AddForce(-relative * damping, ForceMode2D.Force);
        }
    }

    Vector2 Rotate(Vector2 v, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);
        return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos);
    }
}
