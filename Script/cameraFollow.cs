using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    private Vector3 offset = new Vector3(0f, 0f, -10f);
    private float smoothTime = 0.25f;
    private Vector3 velocity = Vector3.zero;

    [SerializeField] private Transform target;

    [Header("Camera Bottom Limit Settings")]
    [SerializeField] private float minDownOffset = 1f;      // how far the camera is allowed to move down
    [SerializeField] private float raycastDistance = 3f;     // how far below the camera to check

    private void Update()
    {
        Vector3 targetPosition = target.position + offset;

        // Raycast downward to detect Ground
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, raycastDistance);

        if (hit.collider != null && hit.collider.CompareTag("Ground"))
        {
            // Camera is NOT allowed to go lower than this Y
            float allowedY = transform.position.y - minDownOffset;

            if (targetPosition.y < allowedY)
                targetPosition.y = allowedY;
        }

        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref velocity,
            smoothTime
        );
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * raycastDistance);
    }
}
