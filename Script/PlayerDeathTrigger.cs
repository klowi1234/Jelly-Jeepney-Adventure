using UnityEngine;

public class PlayerDeath : MonoBehaviour
{
    private GameOverUI gameOverUI;
    private bool hasTriggered = false; // Only allow one trigger

    private void Start()
    {
        // Find the GameOverUI in the scene
        gameOverUI = FindAnyObjectByType<GameOverUI>();

        if (gameOverUI == null)
        {
            Debug.LogError("PlayerDeath: No GameOverUI found in the scene!");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Prevent multiple triggers
        if (hasTriggered) return;

        // Check if the collision is with the Player
        if (collision.CompareTag("Player"))
        {
            Debug.Log("PlayerDeath: Death tile detected! Showing Game Over UI...");
            gameOverUI.ShowGameOver();
            hasTriggered = true; // Mark that we already triggered
        }
    }
}
