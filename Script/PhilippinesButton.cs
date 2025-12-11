using UnityEngine;
using UnityEngine.UI;

public class ButtonLoadScene : MonoBehaviour
{
    public string sceneName;

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            // Use GameManager transition instead of immediate load
            GameManager.Instance.TransitionToScene(sceneName);
        });
    }
}


