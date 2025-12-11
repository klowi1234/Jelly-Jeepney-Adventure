using UnityEngine;
using UnityEngine.U2D;

[ExecuteAlways]
[RequireComponent(typeof(SpriteShapeController))]
public class JellyBodySpriteShape : MonoBehaviour
{
    [Tooltip("Assign all jelly body points (in clockwise order)")]
    public Transform[] jellyPoints;

    [Tooltip("Minimum spacing to prevent SpriteShape from breaking")]
    public float minPointDistance = 0.05f;

    private SpriteShapeController shape;

    void Awake()
    {
        shape = GetComponent<SpriteShapeController>();
    }

    void LateUpdate()
    {
        if (shape == null || jellyPoints == null || jellyPoints.Length < 3)
            return;

        var spline = shape.spline;

        // Make sure spline count matches
        while (spline.GetPointCount() < jellyPoints.Length)
            spline.InsertPointAt(spline.GetPointCount(), Vector3.zero);
        while (spline.GetPointCount() > jellyPoints.Length)
            spline.RemovePointAt(spline.GetPointCount() - 1);

        // ---- FIX: Auto-separate points that are too close ----
        for (int i = 0; i < jellyPoints.Length; i++)
        {
            Vector3 worldPos = jellyPoints[i].position;

            // Compare to previous control point
            int prevIndex = (i - 1 + jellyPoints.Length) % jellyPoints.Length;
            Vector3 prevWorldPos = jellyPoints[prevIndex].position;

            float dist = Vector3.Distance(worldPos, prevWorldPos);

            if (dist < minPointDistance)
            {
                // Push the point outward along the line
                Vector3 dir = (worldPos - prevWorldPos).normalized;
                worldPos = prevWorldPos + dir * minPointDistance;
            }

            Vector3 localPos = transform.InverseTransformPoint(worldPos);

            spline.SetPosition(i, localPos);
            spline.SetTangentMode(i, ShapeTangentMode.Continuous);
        }

        shape.spline.isOpenEnded = false;

#if UNITY_EDITOR
        if (!Application.isPlaying)
            UnityEditor.SceneView.RepaintAll();
#endif
    }
}
