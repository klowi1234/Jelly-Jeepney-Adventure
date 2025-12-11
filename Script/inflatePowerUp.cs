using UnityEngine;

public class InflatePowerUp : MonoBehaviour
{
    public float respawnTime = 5f;
    public float giveDuration = 6f;

    private SpriteRenderer sr;
    private Collider2D col;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        JellyWheelBalloonPhysics[] wheels = other.transform.root.GetComponentsInChildren<JellyWheelBalloonPhysics>();

        if (wheels.Length > 0)
        {
            foreach (var wheel in wheels)
            {
                wheel.canInflate = true;
                wheel.inflatePowerDuration = giveDuration;
            }

            // Start HUD timer
            if (PowerUpHUD.instance != null)
                PowerUpHUD.instance.StartInflateTimer(giveDuration);

            sr.enabled = false;
            col.enabled = false;
            Invoke(nameof(Respawn), respawnTime);
        }
    }

    void Respawn()
    {
        sr.enabled = true;
        col.enabled = true;
    }
}
