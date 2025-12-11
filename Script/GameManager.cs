using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] private GameObject _startingSceneTransition;
    [SerializeField] private GameObject _endingSceneTransition;
    [SerializeField] private float transitionDuration = 1.5f; // how long your animation is

    private AudioManager audioManager; // cache the AudioManager

    private void Awake()
    {
        Instance = this;

        // Cache AudioManager using the new recommended method
        audioManager = Object.FindFirstObjectByType<AudioManager>();
        if (audioManager == null)
        {
            Debug.LogWarning("AudioManager not found in the scene!");
        }
    }

    private void Start()
    {
        // PLAY START TRANSITION
        _startingSceneTransition.SetActive(true);

        // Play portal in SFX
        audioManager?.PlaySFX(audioManager.portalin);

        StartCoroutine(DisableAfterDelay(5f));
    }

    private IEnumerator DisableAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        _startingSceneTransition.SetActive(false);
    }

    // 🔥 MAIN TRANSITION LOGIC
    public void TransitionToScene(string sceneName)
    {
        StartCoroutine(PlayEndTransition(sceneName));
    }

    private IEnumerator PlayEndTransition(string nextScene)
    {
        // PLAY END TRANSITION
        _endingSceneTransition.SetActive(true);

        // Play portal out SFX
        audioManager?.PlaySFX(audioManager.portalout);

        // WAIT FOR ANIMATION
        yield return new WaitForSeconds(transitionDuration);

        // LOAD SCENE
        SceneManager.LoadScene(nextScene);
    }
}
