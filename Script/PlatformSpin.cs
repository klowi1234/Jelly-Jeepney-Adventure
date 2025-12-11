using UnityEngine;

public class RotatingGearForce : MonoBehaviour
{
    public float rotationSpeed = 100f;      // degrees per second (+CCW, -CW)
    public float forceStrength = 30f;       // tune this value
    public ForceMode2D forceMode = ForceMode2D.Force; // or Impulse

    void Update()
    {
        // Rotate visually
        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        Rigidbody2D rb = collision.rigidbody;
        if (rb == null) return;

        // Get the closest point of contact
        Vector2 contactPoint = collision.GetContact(0).point;

        // Vector from gear center to contact point
        Vector2 r = contactPoint - (Vector2)transform.position;

        // Angular speed in radians/sec
        float omega = rotationSpeed * Mathf.Deg2Rad;

        // Tangential direction = perpendicular to r
        Vector2 tangentialDir = new Vector2(-r.y, r.x).normalized;

        // Direction depends on rotation sign
        tangentialDir *= Mathf.Sign(rotationSpeed);

        // Apply force
        rb.AddForce(tangentialDir * forceStrength, forceMode);
    }
}
