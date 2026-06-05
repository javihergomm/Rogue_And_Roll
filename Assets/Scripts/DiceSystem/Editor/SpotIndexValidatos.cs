#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System;

public class SpotIndexValidator
{
    [MenuItem("Tools/Validate Spot Indices")]
    public static void Validate()
    {
        var spots = UnityEngine.Object.FindObjectsByType<Spot>(FindObjectsInactive.Include);
        Array.Sort(spots, (a, b) => a.index.CompareTo(b.index));

        bool ok = true;

        for (int i = 0; i < spots.Length; i++)
        {
            int expected = i + 1;
            if (spots[i].index != expected)
            {
                Debug.LogError("Spot index mismatch: " + spots[i].name +
                               " has index " + spots[i].index +
                               " but expected " + expected);
                ok = false;
            }
        }

        if (ok)
            Debug.Log("All Spot indices are correct from 1 to " + spots.Length);
    }
}
#endif
