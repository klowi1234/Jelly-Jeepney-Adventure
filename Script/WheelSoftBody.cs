using UnityEngine;

public class JellyBodySoftMemory : MonoBehaviour
{
    [Header("Soft Body Points (BODY ONLY)")]
    public Transform centerPoint;
    public Transform[] points;      // BODY perimeter points only
    public Rigidbody2D[] bodies;

    [Header("Shape Memory & Inflation")]
    public float inflateMultiplier = 1.5f;
    public float restoringInflateBoost = 1.2f;
    public float restoringForce = 200f;
    public float antiCollapseForce = 100f;
    public float antiCollapseFalloff = 1.5f;
    public float inflationLevel = 0.5f;

    [Header("Internal Pressure")]
    public float pressureForce = 100f;
    public float pressureBoost = 2f;

    [Header("Collision & Safety")]
    public float minPointDistance = 0.12f;
    public float maxForce = 25f;

    [Header("Ground Check (Optional)")]
    public LayerMask groundLayer;
    public float groundCheckRadius = 0.05f;
    public float minGroundedFraction = 0.2f;

    private Vector2[] baseOffsets;
    private float baseArea;
    private Rigidbody2D centerRB;

    private PolygonCollider2D polyCollider;
    private Vector2[] polyPoints;

    void Start()
    {
        int count = points.Length;
        bodies = new Rigidbody2D[count];
        baseOffsets = new Vector2[count];
        polyPoints = new Vector2[count];

        for (int i = 0; i < count; i++)
        {
            bodies[i] = points[i].GetComponent<Rigidbody2D>();
            if (bodies[i] == null)
                bodies[i] = points[i].gameObject.AddComponent<Rigidbody2D>();

            if (!points[i].TryGetComponent(out CircleCollider2D c))
                points[i].gameObject.AddComponent<CircleCollider2D>();

            baseOffsets[i] = (Vector2)points[i].position - (Vector2)centerPoint.position;
        }

        centerRB = centerPoint.GetComponent<Rigidbody2D>();
        if (centerRB == null)
            centerRB = centerPoint.gameObject.AddComponent<Rigidbody2D>();

        polyCollider = centerPoint.GetComponent<PolygonCollider2D>();
        if (polyCollider == null)
            polyCollider = centerPoint.gameObject.AddComponent<PolygonCollider2D>();

        polyCollider.pathCount = 1;

        baseArea = CalculatePolygonArea();
        UpdatePolygonCollider();
    }

    void FixedUpdate()
    {
        bool airborne = IsMostlyAirborne();

        ApplyMemorySprings(airborne);
        ApplyShapeRetention(airborne);
        ApplyInternalPressure();
        ApplyPointCollisions();
        UpdatePolygonCollider();
        ApplyCenterDamping();
    }

    // ------------------------------------------------------------
    // MEMORY RESTORATION (Jelly returns to original shape)
    // ------------------------------------------------------------
    void ApplyMemorySprings(bool airborne)
    {
        float scale = Mathf.Lerp(1f, inflateMultiplier, inflationLevel);
        float restoreMult = Mathf.Lerp(1f, restoringInflateBoost, inflationLevel);
        float softFactor = airborne ? 0.6f : 1f;

        for (int i = 0; i < points.Length; i++)
        {
            Vector2 targetPos = (Vector2)centerPoint.position + baseOffsets[i] * scale;
            Vector2 correction = targetPos - (Vector2)points[i].position;

            Vector2 force = correction * restoringForce * restoreMult * softFactor * Time.fixedDeltaTime;
            ClampAndApplyForce(bodies[i], force);
        }
    }

    // ------------------------------------------------------------
    // PREVENT THE BODY FROM COLLAPSING INWARD
    // ------------------------------------------------------------
    void ApplyShapeRetention(bool airborne)
    {
        float softFactor = airborne ? 0.6f : 1f;

        for (int i = 0; i < points.Length; i++)
        {
            Vector2 center = centerPoint.position;
            Vector2 pos = points[i].position;
            float currentDist = Vector2.Distance(center, pos);

            float targetDist = baseOffsets[i].magnitude * Mathf.Lerp(1f, inflateMultiplier, inflationLevel);

            if (currentDist < targetDist)
            {
                float compression = (targetDist - currentDist) / targetDist;
                float boost = Mathf.Lerp(1f, antiCollapseFalloff, compression);

                Vector2 f = (pos - center).normalized * antiCollapseForce * compression * boost * softFactor * Time.fixedDeltaTime;
                ClampAndApplyForce(bodies[i], f);
            }
        }
    }

    // ------------------------------------------------------------
    // INTERNAL PRESSURE
    // ------------------------------------------------------------
    void ApplyInternalPressure()
    {
        if (points.Length < 3) return;

        float currentArea = CalculatePolygonArea();
        float ratio = Mathf.Clamp01(currentArea / baseArea);
        float pressure = (baseArea - currentArea) * pressureForce * Mathf.Lerp(1f, pressureBoost, 1f - ratio);

        Vector2 pushTotal = Vector2.zero;

        for (int i = 0; i < points.Length; i++)
        {
            Vector2 dir = ((Vector2)points[i].position - (Vector2)centerPoint.position).normalized;
            Vector2 force = dir * pressure * 0.5f * Time.fixedDeltaTime;

            ClampAndApplyForce(bodies[i], force);
            pushTotal += force;
        }

        ClampAndApplyForce(centerRB, -pushTotal);
    }

    // ------------------------------------------------------------
    // POINT-POINT SPACING
    // ------------------------------------------------------------
    void ApplyPointCollisions()
    {
        for (int i = 0; i < bodies.Length; i++)
        {
            for (int j = i + 1; j < bodies.Length; j++)
            {
                Vector2 delta = bodies[j].position - bodies[i].position;
                float dist = delta.magnitude;

                if (dist < minPointDistance && dist > 0f)
                {
                    float overlap = (minPointDistance - dist) * 0.5f;
                    Vector2 push = delta.normalized * overlap * restoringForce * Time.fixedDeltaTime;

                    ClampAndApplyForce(bodies[i], -push);
                    ClampAndApplyForce(bodies[j], push);
                }
            }
        }
    }

    // ------------------------------------------------------------
    // POLYGON COLLIDER UPDATE
    // ------------------------------------------------------------
    void UpdatePolygonCollider()
    {
        if (polyCollider == null || points.Length < 3) return;

        for (int i = 0; i < points.Length; i++)
            polyPoints[i] = centerPoint.InverseTransformPoint(points[i].position);

        polyCollider.SetPath(0, polyPoints);
    }

    // ------------------------------------------------------------
    // UTILITIES
    // ------------------------------------------------------------
    float CalculatePolygonArea()
    {
        float area = 0f;
        int n = points.Length;

        for (int i = 0; i < n; i++)
        {
            Vector2 p1 = points[i].position;
            Vector2 p2 = points[(i + 1) % n].position;
            area += (p1.x * p2.y - p2.x * p1.y);
        }

        return Mathf.Abs(area) * 0.5f;
    }

    void ClampAndApplyForce(Rigidbody2D rb, Vector2 force)
    {
        if (force.magnitude > maxForce)
            force = force.normalized * maxForce;

        rb.AddForce(force, ForceMode2D.Force);
    }

    bool IsMostlyAirborne()
    {
        int grounded = 0;

        foreach (var p in points)
        {
            if (Physics2D.OverlapCircle(p.position, groundCheckRadius, groundLayer))
                grounded++;
        }

        return ((float)grounded / points.Length) < minGroundedFraction;
    }

    void ApplyCenterDamping()
    {
        centerRB.linearVelocity = new Vector2(centerRB.linearVelocity.x * 0.92f, centerRB.linearVelocity.y);
    }
}
