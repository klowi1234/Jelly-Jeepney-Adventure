using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem; // <-- Required for new Input System

public class VideoEndSceneLoadEndCredits : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public string nextSceneName;

    void Start()
    {
        if (videoPlayer == null)
            videoPlayer = GetComponent<VideoPlayer>();

        videoPlayer.loopPointReached += OnVideoFinished;
    }

    void Update()
    {
        // New Input System: check if space is being held
        if (Keyboard.current.spaceKey.isPressed)
        {
            videoPlayer.playbackSpeed = 2f;
        }
        else
        {
            videoPlayer.playbackSpeed = 1f;
        }
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        SceneManager.LoadScene(nextSceneName);
    }
}
