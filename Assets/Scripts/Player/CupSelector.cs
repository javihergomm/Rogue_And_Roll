using UnityEngine;
using System.Collections.Generic;

/*
 * CupSelector
 * -----------
 * Responsibilities:
 * - Activate cup when a selection area is clicked
 * - Change material color
 * - Move cup to the clicked area
 * - Save selected character
 * - Disable all selection areas after first choice
 */
public class CupSelector : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Renderer cupRenderer;
    [SerializeField] private GameObject cupObject;
    [SerializeField] private ShopExitManager shopExitManager;

    [Header("Material Settings")]
    [SerializeField] private int materialIndexToChange = 0;

    [Header("Selection Areas")]
    [SerializeField] private List<GameObject> selectionAreas = new List<GameObject>();

    public string SelectedCharacter { get; private set; }
    public bool SelectionDone { get; private set; } = false;

    public ShopExitManager ShopExitManager { get { return shopExitManager; } }

    public void ApplySelection(string areaName, Color areaColor, Transform areaTransform)
    {
        Debug.Log("[CupSelector] ApplySelection called with area: " + areaName);

        if (SelectionDone)
        {
            Debug.Log("[CupSelector] Selection already done, ignoring.");
            return;
        }

        if (this == null || this.Equals(null))
        {
            Debug.LogWarning("[CupSelector] CupSelector destroyed or invalid.");
            return;
        }

        // Activate cup
        if (cupObject != null)
        {
            cupObject.SetActive(true);
            Debug.Log("[CupSelector] CupObject activated.");
        }
        else
        {
            Debug.LogWarning("[CupSelector] CupObject reference is missing!");
        }

        // Change material color
        if (cupRenderer != null)
        {
            Material[] mats = cupRenderer.materials;
            if (materialIndexToChange >= 0 && materialIndexToChange < mats.Length)
            {
                mats[materialIndexToChange].color = areaColor;
                cupRenderer.materials = mats;
                Debug.Log("[CupSelector] Cup color changed to: " + areaColor);
            }
            else
            {
                Debug.LogWarning("[CupSelector] Material index out of range.");
            }
        }
        else
        {
            Debug.LogWarning("[CupSelector] CupRenderer reference is missing!");
        }

        // Save selection
        SelectedCharacter = areaName;
        SelectionDone = true;
        Debug.Log("[CupSelector] Selection saved: " + SelectedCharacter);

        // Move cup
        if (cupObject != null)
        {
            cupObject.transform.position = areaTransform.position;
            Debug.Log("[CupSelector] Cup moved to area position: " + areaTransform.position);
        }

        // Disable areas
        foreach (var area in selectionAreas)
        {
            if (area != null)
            {
                area.SetActive(false);
                Debug.Log("[CupSelector] Disabled area: " + area.name);
            }
        }
    }
}
