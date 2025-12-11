using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class JellyWheelBalloonPhysics : MonoBehaviour
{
    [Header("Wheel Setup")]
    public Transform centerPoint;
    public Transform[] points;

    [Header("Drive Settings")]
    public float driveForce = 20f;
    public float maxForce = 200f; // SAFETY clamp for extreme forces

    [Header("Inflation Settings")]
    public float inflateMultiplier = 3.0f;
    public float inflateSpeed = 20f;
    public float deflateSpeed = 8f;
    public float stiffnessMultiplier = 0.6f;

    [Header("Memory Spring Settings")]
    public float restoringForce = 50f;
    public float restoringInflateBoost = 1.4f;

    [Header("Outward Boost")]
    public float outwardBoostForce = 1000f;
    public float outwardBoostFalloff = 0.4f;

    [Header("Shape Retention")]
    public float antiCollapseForce = 800f;
    public float antiCollapseFalloff = 0.4f;

    [Header("Balloon Pressure Settings")]
    public float basePressure = 200f;
    public float maxPressure = 1500f;
    public float pressureSmoothness = 8f;
    public float pressureSpread = 1.2f;

    [Header("Cling Mode")]
    public float clingForce = 1000f;
    public float clingRadius = 0.6f;
    public LayerMask groundLayer;

    [Header("Power Ups (Timed)")]
    public bool canInflate = false;
    public float inflatePowerDuration = 0f;

    public bool canClingToggle = false;
    public float clingPowerDuration = 0f;

    public bool canSpeedBoost = false;
    public float speedBoostDuration = 0f;
    public float speedMultiplier = 2f;

    private bool clingMode = false;

    private Rigidbody2D[] bodies;
    private SpringJoint2D[] radiusSprings;
    private DistanceJoint2D[] radiusDistances;
    private List<DistanceJoint2D> circleLinks;
    private List<float> baseCircleDistances;
    private float[] baseSpringDistances;
    private float[] baseSpringFrequencies;
    private float[] baseDistanceJoints;
    private Vector2[] baseOffsets;

    private float inflationLevel = 0f;
    private float internalPressure = 0f;

    private AudioManager audioManager;
    private bool wasInflating = false;

    private AudioSource movementSFXSource;
    private float movementInput = 0f;

    void Start()
    {
        audioManager = Object.FindFirstObjectByType<AudioManager>();
        if (audioManager == null) Debug.LogWarning("AudioManager not found!");

        int count = points.Length;
        bodies = new Rigidbody2D[count];
        radiusSprings = new SpringJoint2D[count];
        radiusDistances = new DistanceJoint2D[count];
        baseSpringDistances = new float[count];
        baseSpringFrequencies = new float[count];
        baseDistanceJoints = new float[count];
        baseOffsets = new Vector2[count];

        circleLinks = new List<DistanceJoint2D>();
        baseCircleDistances = new List<float>();

        for (int i = 0; i < count; i++)
        {
            bodies[i] = points[i].GetComponent<Rigidbody2D>();
            baseOffsets[i] = points[i].position - centerPoint.position;

            bodies[i].collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            bodies[i].sleepMode = RigidbodySleepMode2D.NeverSleep;

            foreach (var sj in points[i].GetComponents<SpringJoint2D>())
            {
                if (sj.connectedBody != null && sj.connectedBody.transform == centerPoint)
                {
                    radiusSprings[i] = sj;
                    baseSpringDistances[i] = sj.distance;
                    baseSpringFrequencies[i] = sj.frequency;
                    sj.autoConfigureDistance = false;
                    sj.dampingRatio = 0.7f;
                    break;
                }
            }

            foreach (var dj in points[i].GetComponents<DistanceJoint2D>())
            {
                if (dj.connectedBody != null && dj.connectedBody.transform == centerPoint)
                {
                    radiusDistances[i] = dj;
                    baseDistanceJoints[i] = dj.distance;
                    dj.autoConfigureDistance = false;
                    break;
                }
            }

            foreach (var dj in points[i].GetComponents<DistanceJoint2D>())
            {
                if (dj.connectedBody != null && dj.connectedBody.transform != centerPoint)
                {
                    circleLinks.Add(dj);
                    baseCircleDistances.Add(dj.distance);
                }
            }

            if (points[i].GetComponent<CircleCollider2D>() == null)
            {
                var col = points[i].gameObject.AddComponent<CircleCollider2D>();
                col.radius = 0.1f;
                col.isTrigger = false;
            }
        }

        if (audioManager != null && audioManager.leftandright != null)
        {
            movementSFXSource = gameObject.AddComponent<AudioSource>();
            movementSFXSource.clip = audioManager.leftandright;
            movementSFXSource.loop = true;
            movementSFXSource.playOnAwake = false;
            movementSFXSource.volume = 0f;
            movementSFXSource.pitch = 1f;
        }
    }

    void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        if (inflatePowerDuration > 0f) { inflatePowerDuration -= Time.deltaTime; if (inflatePowerDuration <= 0f) canInflate = false; }
        if (clingPowerDuration > 0f) { clingPowerDuration -= Time.deltaTime; if (clingPowerDuration <= 0f) { canClingToggle = false; clingMode = false; } }
        if (speedBoostDuration > 0f) { speedBoostDuration -= Time.deltaTime; if (speedBoostDuration <= 0f) canSpeedBoost = false; }

        bool inflating = canInflate && kb.spaceKey.isPressed;

        if (audioManager != null)
        {
            if (inflating && !wasInflating) audioManager.PlaySFX(audioManager.inflate);
            else if (!inflating && wasInflating) audioManager.PlaySFX(audioManager.deflate);
        }
        wasInflating = inflating;

        inflationLevel = Mathf.MoveTowards(inflationLevel, inflating ? 1f : 0f, Time.deltaTime * (inflating ? inflateSpeed : deflateSpeed));
        float targetPressure = Mathf.Lerp(basePressure, maxPressure, inflationLevel);
        internalPressure = Mathf.Lerp(internalPressure, targetPressure, Time.deltaTime * pressureSmoothness);

        ApplyInflationDynamic(inflationLevel);

        if (canClingToggle && kb.eKey.wasPressedThisFrame)
        {
            clingMode = !clingMode;
            if (clingMode && audioManager != null) audioManager.PlaySFX(audioManager.sticky);
        }

        movementInput = 0f;
        if (kb.aKey.isPressed) movementInput += 1f;
        if (kb.dKey.isPressed) movementInput -= 1f;

        HandleMovementSFX();
    }

    void FixedUpdate()
    {
        float currentDriveForce = driveForce;
        if (canSpeedBoost) currentDriveForce *= speedMultiplier;

        if (Mathf.Abs(movementInput) > 0f)
        {
            for (int i = 0; i < bodies.Length; i++)
            {
                Rigidbody2D rb = bodies[i];
                Vector2 dir = rb.position - (Vector2)centerPoint.position;
                Vector2 tangent = new Vector2(-dir.y, dir.x).normalized;
                float wheelDriveMultiplier = (i < 2) ? 0.8f : 1f;
                ApplyClampedForce(rb, tangent * currentDriveForce * movementInput * wheelDriveMultiplier * Time.fixedDeltaTime);
            }
        }

        ApplyMemorySprings();
        ApplyShapeRetention();
        ApplyInternalPressureDynamic();

        if (clingMode) ApplyClingEffect();
        PreventWheelOverlap();
    }

    // ---------------- Physics Methods ----------------
    void ApplyInflationDynamic(float level)
    {
        Vector2 dynamicCenter = ComputeDynamicCenter();

        for (int i = 0; i < points.Length; i++)
        {
            float sizeScale = Mathf.Lerp(1f, inflateMultiplier, level);
            float stiffnessScale = Mathf.Lerp(1f, stiffnessMultiplier, Mathf.Sqrt(level));
            float wheelOutwardMultiplier = (i < 2) ? 0.5f : 1f;

            Vector2 dir = ((Vector2)points[i].position - dynamicCenter).normalized;
            float falloff = Mathf.Lerp(1f, outwardBoostFalloff, level);
            float inflateDamping = Mathf.Lerp(1f, 0.25f, level);

            if (radiusSprings[i] != null)
            {
                radiusSprings[i].distance = baseSpringDistances[i] * sizeScale;
                radiusSprings[i].frequency = baseSpringFrequencies[i] * stiffnessScale;
            }

            if (radiusDistances[i] != null)
                radiusDistances[i].distance = baseDistanceJoints[i] * sizeScale;

            Vector2 force = dir * outwardBoostForce * level * falloff * inflateDamping * wheelOutwardMultiplier * Time.fixedDeltaTime;
            ApplyClampedForce(bodies[i], force);
        }

        for (int i = 0; i < circleLinks.Count; i++)
        {
            if (circleLinks[i] != null)
            {
                float targetDistance = baseCircleDistances[i] * Mathf.Lerp(1f, inflateMultiplier, level);
                targetDistance = Mathf.Max(targetDistance, 0.15f);
                circleLinks[i].distance = targetDistance;
            }
        }
    }

    void ApplyMemorySprings()
    {
        float scale = Mathf.Lerp(1f, inflateMultiplier, inflationLevel);
        float restoreMult = Mathf.Lerp(1f, restoringInflateBoost, inflationLevel);

        for (int i = 0; i < points.Length; i++)
        {
            Vector2 targetPos = (Vector2)centerPoint.position + baseOffsets[i] * scale;
            Vector2 correction = targetPos - (Vector2)points[i].position;
            float wheelRestoreMultiplier = (i < 2) ? 0.6f : 1f;
            float softFactor = 1f;

            foreach (var other in bodies)
            {
                if (other == bodies[i]) continue;
                float dist = Vector2.Distance(points[i].position, other.position);
                if (dist < 0.15f) softFactor *= 0.3f;
            }

            Vector2 force = correction * restoringForce * restoreMult * wheelRestoreMultiplier * softFactor * Time.fixedDeltaTime;
            ApplyClampedForce(bodies[i], force);
        }
    }

    void ApplyShapeRetention()
    {
        for (int i = 0; i < points.Length; i++)
        {
            Vector2 center = centerPoint.position;
            Vector2 pos = points[i].position;
            Vector2 dir = (pos - center).normalized;

            float currentDist = Vector2.Distance(center, pos);
            float baseDist = baseSpringDistances[i] * Mathf.Lerp(1f, inflateMultiplier, inflationLevel);

            if (currentDist < baseDist)
            {
                float compression = (baseDist - currentDist) / baseDist;
                float collisionFactor = 1f;

                foreach (var other in bodies)
                {
                    if (other == bodies[i]) continue;
                    float dist = Vector2.Distance(pos, other.position);
                    if (dist < 0.15f) collisionFactor = 0.3f;
                }

                Vector2 force = dir * antiCollapseForce * Mathf.Lerp(1f, antiCollapseFalloff, compression) * compression * collisionFactor * Time.fixedDeltaTime;
                ApplyClampedForce(bodies[i], force);
            }
        }
    }

    void ApplyInternalPressureDynamic()
    {
        Vector2 dynamicCenter = ComputeDynamicCenter();

        for (int i = 0; i < points.Length; i++)
        {
            Vector2 dir = ((Vector2)points[i].position - dynamicCenter).normalized;
            ApplyClampedForce(bodies[i], dir * internalPressure * Time.fixedDeltaTime);
        }

        for (int i = 0; i < circleLinks.Count; i++)
        {
            var link = circleLinks[i];
            if (link == null) continue;

            Rigidbody2D a = link.GetComponent<Rigidbody2D>();
            Rigidbody2D b = link.connectedBody;
            if (a == null || b == null) continue;

            Vector2 dir = (a.position - b.position).normalized;
            ApplyClampedForce(a, dir * internalPressure * pressureSpread * Time.fixedDeltaTime);
            ApplyClampedForce(b, -dir * internalPressure * pressureSpread * Time.fixedDeltaTime);
        }
    }

    Vector2 ComputeDynamicCenter()
    {
        Vector2 sum = Vector2.zero;
        for (int i = 0; i < points.Length; i++)
            sum += (Vector2)points[i].position;
        return sum / points.Length;
    }

    void ApplyClingEffect()
    {
        if (!canClingToggle) return;

        foreach (var rb in bodies)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(rb.position, clingRadius, groundLayer);
            foreach (var hit in hits)
            {
                if (hit == null || hit.attachedRigidbody == rb) continue;

                Vector2 closest = hit.ClosestPoint(rb.position);
                Vector2 normal = (rb.position - closest).normalized;
                float distance = Vector2.Distance(rb.position, closest);

                if (distance < clingRadius)
                {
                    float clingStrength = clingForce * (1f - distance / clingRadius);
                    ApplyClampedForce(rb, -normal * clingStrength * Time.fixedDeltaTime);

                    Vector2 vel = rb.linearVelocity;
                    float velAlongNormal = Vector2.Dot(vel, normal);
                    rb.linearVelocity -= normal * velAlongNormal * 0.7f * Time.fixedDeltaTime;
                }
            }
        }
    }

    void PreventWheelOverlap()
    {
        float safetyRadius = 0.1f;
        foreach (var rb in bodies)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(rb.position, safetyRadius, groundLayer);
            foreach (var hit in hits)
            {
                if (hit == null || hit.attachedRigidbody == rb) continue;

                Vector2 closest = hit.ClosestPoint(rb.position);
                Vector2 dir = rb.position - closest;
                float dist = dir.magnitude;
                if (dist < 0.05f) dist = 0.05f;
                Vector2 correction = dir.normalized * (safetyRadius - dist);
                rb.position += correction;
                rb.linearVelocity *= 0.5f;
            }
        }
    }

    void ApplyClampedForce(Rigidbody2D rb, Vector2 force)
    {
        if (force.magnitude > maxForce)
            force = force.normalized * maxForce;
        rb.AddForce(force, ForceMode2D.Force);
    }

    void HandleMovementSFX()
    {
        if (movementSFXSource == null) return;

        if (Mathf.Abs(movementInput) > 0f)
        {
            if (!movementSFXSource.isPlaying) movementSFXSource.Play();
            movementSFXSource.pitch = Mathf.Lerp(1f, 1.5f, Mathf.Abs(movementInput));
            movementSFXSource.volume = Mathf.Lerp(0.3f, 1f, Mathf.Abs(movementInput));
        }
        else
        {
            movementSFXSource.volume = 0f;
            if (movementSFXSource.isPlaying) movementSFXSource.Stop();
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = clingMode ? Color.green : Color.yellow;
        if (points == null) return;
        foreach (var p in points)
        {
            if (p != null)
                Gizmos.DrawWireSphere(p.position, clingRadius);
        }
    }
}
