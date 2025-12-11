using UnityEngine;

public class DoorLock : MonoBehaviour
{
    public Door door;
    public string requiredLevel;

    private void OnEnable()
    {
        // If required level is NOT complete → lock door
        if (!ProgressManager.instance.IsLevelComplete(requiredLevel))
        {
            door.isLocked = true;
            GetComponent<SpriteRenderer>().color = Color.gray; // visual lock
        }
        else
        {
            // Level completed → unlock door
            door.isLocked = false;
            GetComponent<SpriteRenderer>().color = Color.white; // visual unlock
        }
    }
}
