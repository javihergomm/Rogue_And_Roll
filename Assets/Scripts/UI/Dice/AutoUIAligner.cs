using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

[ExecuteAlways]
public class AutoUIAlignerWithPreview : MonoBehaviour
{
    [Header("UI References")]
    public RectTransform inventoryButton;
    public RectTransform diceResultsRoot;

    [Header("Prefab")]
    public GameObject rowPrefab;

    [Header("Layout")]
    public float verticalOffset = 20f;
    public float rowSpacing = 70f;

    [Header("Simulacion")]
    public int numberOfDice = 3;
    public List<int> simulatedMaxRolls = new();
    public List<int> simulatedResults = new();
    public List<string> simulatedEffects = new();

    private List<GameObject> previewRows = new();
    private readonly int[] possibleDice = new int[] { 4, 6, 8, 20 };

    private void OnEnable()
    {
        RefreshPreview();
    }

    private void OnDisable()
    {
        ClearPreview();
    }

    private void Update()
    {
        if (!Application.isPlaying)
        {
            AlignUnderButton();
            RefreshPreview();
        }
    }

    private void AlignUnderButton()
    {
        if (inventoryButton == null || diceResultsRoot == null)
            return;

        if (diceResultsRoot.parent != inventoryButton)
            diceResultsRoot.SetParent(inventoryButton, false);

        diceResultsRoot.anchorMin = new Vector2(0f, 1f);
        diceResultsRoot.anchorMax = new Vector2(0f, 1f);
        diceResultsRoot.pivot = new Vector2(0f, 1f);

        float leftX = -inventoryButton.rect.width * inventoryButton.pivot.x;

        diceResultsRoot.anchoredPosition = new Vector2(
            leftX,
            -inventoryButton.rect.height - verticalOffset
        );

        // FIX: evitar que todo se escale x2
        diceResultsRoot.localScale = Vector3.one;

        // FIX: asegurar que cada fila tambien tenga escala 1
        foreach (Transform child in diceResultsRoot)
            child.localScale = Vector3.one;
    }


    public void RefreshPreview()
    {
        if (rowPrefab == null)
            return;

        EnsureListSize(simulatedMaxRolls, numberOfDice);
        EnsureListSize(simulatedResults, numberOfDice);
        EnsureListSize(simulatedEffects, numberOfDice);

        ClearPreview();

        for (int i = 0; i < numberOfDice; i++)
        {
            GameObject row = CreatePreviewRow(simulatedMaxRolls[i], simulatedResults[i], simulatedEffects[i]);
            row.transform.SetParent(diceResultsRoot, false);

            RectTransform rt = row.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);

            rt.anchoredPosition = new Vector2(0, -i * rowSpacing);

            previewRows.Add(row);
        }
    }

    public void RandomizeDiceTypes()
    {
        for (int i = 0; i < numberOfDice; i++)
            simulatedMaxRolls[i] = possibleDice[Random.Range(0, possibleDice.Length)];
    }

    public void RollAll()
    {
        for (int i = 0; i < numberOfDice; i++)
            simulatedResults[i] = Random.Range(1, simulatedMaxRolls[i] + 1);
    }

    public void ResetAll()
    {
        for (int i = 0; i < numberOfDice; i++)
        {
            simulatedResults[i] = 0;
            simulatedEffects[i] = "";
        }
    }

    private void EnsureListSize<T>(List<T> list, int size)
    {
        while (list.Count < size)
            list.Add(default);
        while (list.Count > size)
            list.RemoveAt(list.Count - 1);
    }

    private void ClearPreview()
    {
        foreach (var row in previewRows)
            if (row != null)
                DestroyImmediate(row);

        previewRows.Clear();
    }

    private GameObject CreatePreviewRow(int maxRoll, int result, string effect)
    {
        bool isRolled = result != 0;

        GameObject row = Instantiate(rowPrefab);
        RectTransform rt = row.GetComponent<RectTransform>();
        rt.localScale = Vector3.one;

        // Altura compacta
        rt.sizeDelta = isRolled ? new Vector2(350, 70) : new Vector2(350, 40);

        // Referencias internas del prefab
        var nameTMP = row.transform.Find("NameText").GetComponent<TextMeshProUGUI>();
        var img = row.transform.Find("Image").GetComponent<Image>();
        var imgRT = img.GetComponent<RectTransform>();
        var effTMP = row.transform.Find("EffectsText").GetComponent<TextMeshProUGUI>();
        var effRT = effTMP.GetComponent<RectTransform>();

        //
        // POSICIONAR IMAGEN A LA IZQUIERDA
        //
        if (isRolled)
        {
            img.enabled = true;
            img.sprite = DiceFaceExtractor.GetFace(maxRoll, result);

            imgRT.anchorMin = new Vector2(0f, 1f);
            imgRT.anchorMax = new Vector2(0f, 1f);
            imgRT.pivot = new Vector2(0f, 1f);

            imgRT.sizeDelta = new Vector2(50, 50);
            imgRT.anchoredPosition = new Vector2(0, -10);
        }
        else
        {
            img.enabled = false;
            imgRT.sizeDelta = Vector2.zero;
        }

        //
        // NAME TEXT (lo dejamos pero lo movemos bien)
        //
        nameTMP.text = isRolled ? "" : "Sin tirar";

        var nameRT = nameTMP.GetComponent<RectTransform>();
        nameRT.anchorMin = new Vector2(0f, 1f);
        nameRT.anchorMax = new Vector2(0f, 1f);
        nameRT.pivot = new Vector2(0f, 1f);

        // Si no hay tirada, va debajo del borde superior
        nameRT.anchoredPosition = isRolled
            ? new Vector2(60, 0)   // a la derecha de la imagen
            : new Vector2(0, 0);   // sin imagen, empieza a la izquierda

        //
        // EFFECTS TEXT
        //
        effTMP.text = isRolled
            ? "- " + (string.IsNullOrEmpty(effect) ? "ninguno" : effect)
            : "";

        effRT.anchorMin = new Vector2(0f, 1f);
        effRT.anchorMax = new Vector2(0f, 1f);
        effRT.pivot = new Vector2(0f, 1f);

        effRT.sizeDelta = new Vector2(260, 40);

        effRT.anchoredPosition = isRolled
            ? new Vector2(60, -10)   // debajo del NameText, a la derecha de la imagen
            : new Vector2(0, -10);   // sin imagen, centrado debajo

        return row;
    }

}
