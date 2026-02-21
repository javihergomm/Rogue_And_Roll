using UnityEngine;

/*
 * AutoCenterModel
 * ----------------
 * Normalizes a 3D model inside its own container:
 * - Applies item-specific rotation, scale and height offsets
 * - Computes bounds using real mesh vertices
 * - Recenters horizontally (X/Z)
 * - Aligns the base to Y = 0
 * 
 */
public class AutoCenterModel : MonoBehaviour
{
    public void Normalize(BaseItemSO item)
    {
        if (item == null)
            return;

        // Reset transform
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;

        // Apply item-specific rotation
        transform.localRotation = Quaternion.Euler(item.StoreRotationOffset);

        // Compute bounds after rotation
        Bounds b = ComputeLocalBounds();

        // Scale using item multiplier
        float largest = Mathf.Max(b.size.x, b.size.y, b.size.z);
        if (largest < 0.0001f) largest = 1f;

        float scaleFactor = item.StoreScaleMultiplier;
        transform.localScale = Vector3.one * scaleFactor;

        // Recompute bounds after scaling
        b = ComputeLocalBounds();

        // Align base to Y = 0
        float bottomY = b.min.y;
        Vector3 p = transform.localPosition;
        p.y -= bottomY;
        transform.localPosition = p;

        // Apply height offset
        p = transform.localPosition;
        p.y += item.StoreHeightOffset;
        transform.localPosition = p;

        // Final horizontal centering
        b = ComputeLocalBounds();
        Vector3 horizontalOffset = new Vector3(b.center.x, 0f, b.center.z);
        transform.localPosition -= horizontalOffset;

        // Apply X and Z offsets
        p = transform.localPosition;
        p.x += item.StoreXPositionOffset;
        p.z += item.StoreZPositionOffset;
        transform.localPosition = p;
    }


    private Bounds ComputeLocalBounds()
    {
        MeshFilter[] filters = GetComponentsInChildren<MeshFilter>();
        Bounds combined = new Bounds();
        bool first = true;

        foreach (var f in filters)
        {
            Mesh mesh = f.sharedMesh;
            if (mesh == null) continue;

            foreach (var v in mesh.vertices)
            {
                Vector3 worldV = f.transform.TransformPoint(v);
                Vector3 localV = transform.InverseTransformPoint(worldV);

                if (first)
                {
                    combined = new Bounds(localV, Vector3.zero);
                    first = false;
                }
                else
                {
                    combined.Encapsulate(localV);
                }
            }
        }

        return combined;
    }
}
