using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private GameObject gameOverPanel;

    private void Start()
    {
        // Make sure the Game Over UI is hidden at the start
        gameOverPanel.SetActive(false);
    }

    public void ShowGameOver()
    {
        gameOverPanel.SetActive(true);
        Time.timeScale = 0f; // Pause the game
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        GameManager.Instance.TransitionToScene(SceneManager.GetActiveScene().name);
    }
}

