using UnityEngine;

public class LevelExit : MonoBehaviour
{
    public string levelName;   // Example: "Luzon1"
    public string returnScene; // Example: "LuzonLevels"

    private bool hasTriggered = false; // Only allow one trigger

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Prevent multiple triggers
        if (hasTriggered) return;

        if (other.CompareTag("Player"))
        {
            ProgressManager.instance.CompleteLevel(levelName);

            // Instead of loading scene directly:
            GameManager.Instance.TransitionToScene(returnScene);

            hasTriggered = true; // Mark as triggered
        }
    }
}
