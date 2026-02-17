using UnityEngine;

/*
 * AutoCenterModel
 * ----------------
 * Automatically centers a 3D model so that:
 *  - Its pivot is centered horizontally
 *  - Its base sits exactly on Y = 0
 *  - It works with any imported prefab (FBX, OBJ, etc.)
 *
 * Attach this script to the parent object that contains the model.
 */
public class AutoCenterModel : MonoBehaviour
{
    [Header("Optional extra offset")]
    public float extraYOffset = 0f;

    private void Start()
    {
        CenterModel();
    }

    public void CenterModel()
    {
        MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();

        if (renderers.Length == 0)
            return;

        // Calculate combined bounds in world space
        Bounds combined = renderers[0].bounds;
        foreach (var r in renderers)
            combined.Encapsulate(r.bounds);

        // Convert bounds center to local space
        Vector3 localCenter = transform.InverseTransformPoint(combined.center);

        // Move model so its center is at local zero
        foreach (Transform child in transform)
            child.localPosition -= localCenter;

        // Recalculate bounds after centering
        combined = new Bounds();
        foreach (var r in renderers)
            combined.Encapsulate(r.bounds);

        // Move model up so the bottom sits on Y = 0
        float bottomY = transform.InverseTransformPoint(combined.min).y;

        foreach (Transform child in transform)
        {
            Vector3 p = child.localPosition;
            p.y -= bottomY;
            p.y += extraYOffset;
            child.localPosition = p;
        }
    }
}
