using UnityEngine;
using UnityEngine.InputSystem; // <-- IMPORTANT

public class Door : MonoBehaviour
{
    public string sceneToLoad;
    public bool isLocked = false;

    private bool playerInRange = false;

    void Update()
    {
        if (playerInRange && !isLocked)
        {
            // New Input System key check
            if (Keyboard.current != null &&
                (Keyboard.current.eKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame))
            {
                // Instead of loading scene directly:
                GameManager.Instance.TransitionToScene(sceneToLoad);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
    }
}
