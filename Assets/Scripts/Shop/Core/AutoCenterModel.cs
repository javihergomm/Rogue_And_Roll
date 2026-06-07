using UnityEngine;

public class AutoCenterModel : MonoBehaviour
{
    // Normalizes position, rotation and scale based on item settings
    public void Normalize(BaseItemSO item)
    {
        if (item == null)
            return;

        // Reset position and apply rotation
        transform.SetLocalPositionAndRotation(
            Vector3.zero,
            Quaternion.Euler(item.StoreRotationOffset)
        );

        // Apply scale
        transform.localScale = Vector3.one * item.StoreScaleMultiplier;

        // Compute bounds after rotation and scale
        Bounds b = ComputeLocalBounds();

        Vector3 p = transform.localPosition;

        // Move base to Y = 0
        p.y -= b.min.y;

        // Apply height offset
        p.y += item.StoreHeightOffset;

        // Recompute bounds to center horizontally
        b = ComputeLocalBounds();
        p.x -= b.center.x;
        p.z -= b.center.z;

        // Apply item X/Z offsets
        p.x += item.StoreXPositionOffset;
        p.z += item.StoreZPositionOffset;

        // Final position
        transform.localPosition = p;
    }

    // Computes combined local bounds of all MeshFilters
    private Bounds ComputeLocalBounds()
    {
        MeshFilter[] filters = GetComponentsInChildren<MeshFilter>();
        Bounds combined = new Bounds();
        bool first = true;

        foreach (var f in filters)
        {
            Mesh mesh = f.sharedMesh;
            if (mesh == null)
                continue;

            Bounds b = mesh.bounds;

            Vector3 min = b.min;
            Vector3 max = b.max;

            Vector3[] corners = new Vector3[8];
            corners[0] = new Vector3(min.x, min.y, min.z);
            corners[1] = new Vector3(max.x, min.y, min.z);
            corners[2] = new Vector3(min.x, max.y, min.z);
            corners[3] = new Vector3(max.x, max.y, min.z);
            corners[4] = new Vector3(min.x, min.y, max.z);
            corners[5] = new Vector3(max.x, min.y, max.z);
            corners[6] = new Vector3(min.x, max.y, max.z);
            corners[7] = new Vector3(max.x, max.y, max.z);

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
