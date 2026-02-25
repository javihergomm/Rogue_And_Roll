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

            // Get the mesh's local bounds
            Bounds b = mesh.bounds;

            // Compute the 8 corners of the bounding box
            Vector3[] corners = new Vector3[8];
            Vector3 min = b.min;
            Vector3 max = b.max;

            corners[0] = new Vector3(min.x, min.y, min.z);
            corners[1] = new Vector3(max.x, min.y, min.z);
            corners[2] = new Vector3(min.x, max.y, min.z);
            corners[3] = new Vector3(max.x, max.y, min.z);
            corners[4] = new Vector3(min.x, min.y, max.z);
            corners[5] = new Vector3(max.x, min.y, max.z);
            corners[6] = new Vector3(min.x, max.y, max.z);
            corners[7] = new Vector3(max.x, max.y, max.z);

            // Transform corners into this object's local space
            for (int i = 0; i < 8; i++)
            {
                Vector3 world = f.transform.TransformPoint(corners[i]);
                Vector3 local = transform.InverseTransformPoint(world);

                if (first)
                {
                    combined = new Bounds(local, Vector3.zero);
                    first = false;
                }
                else
                {
                    combined.Encapsulate(local);
                }
            }
        }

        return combined;
    }

}
