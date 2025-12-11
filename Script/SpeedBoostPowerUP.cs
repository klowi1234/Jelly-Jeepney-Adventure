using UnityEngine;

public class SpeedBoostPowerUp : MonoBehaviour
{
    public float respawnTime = 5f;      // Time before pickup respawns
    public float boostDuration = 5f;    // How long the speed boost lasts
    public float speedMultiplier = 2f;  // How much faster the wheel moves

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
                wheel.canSpeedBoost = true;
                wheel.speedBoostDuration = boostDuration;
                wheel.speedMultiplier = speedMultiplier;
            }

            // Start HUD timer
            if (PowerUpHUD.instance != null)
                PowerUpHUD.instance.StartSpeedBoostTimer(boostDuration);

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
