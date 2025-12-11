using UnityEngine;
using UnityEngine.InputSystem;

public class ProgressManager : MonoBehaviour
{
    public static ProgressManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void CompleteLevel(string levelName)
    {
        PlayerPrefs.SetInt(levelName, 1);
        PlayerPrefs.Save();
    }

    public bool IsLevelComplete(string levelName)
    {
        return PlayerPrefs.GetInt(levelName, 0) == 1;
    }

    void Update()
    {
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            ResetProgress();
            Debug.Log("✅ Progress Reset (Pressed R)");
        }
    }

    // ✅ Call this to reset everything
    public void ResetProgress()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("✅ Progress Reset!");
    }
}
