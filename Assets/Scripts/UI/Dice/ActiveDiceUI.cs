using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class ActiveDiceUI : MonoBehaviour
{
    [Header("UI")]
    public RectTransform resultsRoot;
    public GameObject rowPrefab;

    private RectTransform diceBlock;
    private RectTransform summaryBlock;

    private int lastMovement = 0;
    private string lastEffects = "";
    private bool lastWasEnemy = false;
    private bool hasSummary = false;

    private void OnEnable()
    {
        StartCoroutine(DelayedInit());
        TurnManager.OnEnemyRollCalculated += HandleEnemyRoll;
    }

    private void OnDisable()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnActiveDiceChanged -= RefreshUI;

        TurnManager.OnEnemyRollCalculated -= HandleEnemyRoll;
    }

    private IEnumerator DelayedInit()
    {
        yield return null;

        diceBlock = CreateBlock("DiceBlock");
        summaryBlock = CreateBlock("SummaryBlock");

        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnActiveDiceChanged += RefreshUI;

        RefreshUI();
    }

    private RectTransform CreateBlock(string name)
    {
        var go = new GameObject(name, typeof(RectTransform));
        var rt = go.GetComponent<RectTransform>();
        rt.SetParent(resultsRoot, false);

        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);

        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(420, 0);

        return rt;
    }

    private void HandleEnemyRoll(int total)
    {
        lastWasEnemy = true;
        lastMovement = total;
        lastEffects = "";
        hasSummary = true;

        RefreshUI();
    }

    public void SetLastTurnSummary(int movement, string effects, bool wasEnemy)
    {
        lastMovement = movement;
        lastEffects = effects ?? "";
        lastWasEnemy = wasEnemy;
        hasSummary = true;

        RefreshUI();
    }

    private void RefreshUI()
    {
        if (diceBlock == null || summaryBlock == null)
            return;

        bool isPlayerTurn = TurnManager.Instance.IsPlayerTurn();

        foreach (Transform t in diceBlock)
            Destroy(t.gameObject);

        CreateHeader(isPlayerTurn ? "Jugador" : "Enemigo");

        if (!isPlayerTurn)
        {
            CreateSimpleRow("Turno del enemigo...");
        }
        else
        {
            var slots = InventoryManager.Instance.ActiveDice.Slots;
            bool anyDice = false;

            foreach (var slot in slots)
            {
                if (slot == null || slot.Quantity == 0)
                    continue;

                anyDice = true;
                CreateDiceRow(slot);
            }

            if (!anyDice)
                CreateSimpleRow("Sin tirar");
        }

        RedrawSummary();

        StartCoroutine(RepositionNextFrame());
    }

    private IEnumerator RepositionNextFrame()
    {
        yield return null;

        RepositionDiceRows();
        RepositionSummary();
    }

    private void RedrawSummary()
    {
        foreach (Transform t in summaryBlock)
            Destroy(t.gameObject);

        if (!hasSummary)
            return;

        var row = Instantiate(rowPrefab, summaryBlock);

        string title = lastWasEnemy ? "Turno anterior (enemigo)" : "Turno anterior";
        string text = "Mov: " + lastMovement + "  |  Efectos: " +
                      (string.IsNullOrEmpty(lastEffects) ? "ninguno" : lastEffects);

        SetupRow(row, title, text, false, true);

        var nameTMP = row.transform.Find("NameText").GetComponent<TextMeshProUGUI>();
        nameTMP.rectTransform.sizeDelta = new Vector2(380, nameTMP.rectTransform.sizeDelta.y);
    }

    private void CreateHeader(string text)
    {
        var row = Instantiate(rowPrefab, diceBlock);
        SetupRow(row, text, "", false, false);
    }

    private void CreateSimpleRow(string text)
    {
        var row = Instantiate(rowPrefab, diceBlock);
        SetupRow(row, "", text, false, false);
    }

    private void CreateDiceRow(ItemSlot slot)
    {
        var row = Instantiate(rowPrefab, diceBlock);

        var rollInfo = DiceRollManager.Instance.GetRollInfo(slot);

        if (!rollInfo.HasValue)
        {
            SetupRow(row, "", "Sin tirar", false, false);
            return;
        }

        DiceSO dice = slot.ItemSO as DiceSO;
        if (dice == null)
        {
            SetupRow(row, "", "Error: dado no encontrado", false, false);
            return;
        }

        int baseRoll = rollInfo.Value.baseRoll;
        int maxRoll = dice.GetMaxFaceValue();

        var img = row.transform.Find("Image").GetComponent<Image>();
        img.sprite = DiceFaceExtractor.GetFace(maxRoll, baseRoll);
        img.enabled = img.sprite != null;

        var effects = DiceRollManager.Instance.GetAppliedEffects(slot);

        string effText = effects.Count == 0 ? "- ninguno" : "";
        foreach (var e in effects)
            effText += "- " + e + "\n";

        SetupRow(row, "", effText, true, false);
    }

    private void SetupRow(GameObject row, string nameText, string effectsText, bool rolled, bool isSummary)
    {
        var nameTMP = row.transform.Find("NameText").GetComponent<TextMeshProUGUI>();
        var effTMP = row.transform.Find("EffectsText").GetComponent<TextMeshProUGUI>();
        var img = row.transform.Find("Image").GetComponent<Image>();

        nameTMP.text = nameText;
        effTMP.text = effectsText;

        nameTMP.lineSpacing = -10f;
        effTMP.lineSpacing = -10f;

        var rt = row.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);

        rt.sizeDelta = new Vector2(420, rolled ? 55 : 28);

        PositionRow(nameTMP.rectTransform, effTMP.rectTransform, img.rectTransform, rolled, isSummary);
    }

    private void PositionRow(RectTransform nameRT, RectTransform effRT, RectTransform imgRT, bool rolled, bool isSummary)
    {
        imgRT.anchorMin = imgRT.anchorMax = imgRT.pivot = new Vector2(0f, 1f);
        nameRT.anchorMin = nameRT.anchorMax = nameRT.pivot = new Vector2(0f, 1f);
        effRT.anchorMin = effRT.anchorMax = effRT.pivot = new Vector2(0f, 1f);

        // SUMMARY FIX: independent layout
        if (isSummary)
        {
            imgRT.sizeDelta = Vector2.zero;
            nameRT.anchoredPosition = new Vector2(4, -2);
            effRT.anchoredPosition = new Vector2(4, -30); // guaranteed separation
            return;
        }

        if (rolled)
        {
            imgRT.sizeDelta = new Vector2(40, 40);
            imgRT.anchoredPosition = new Vector2(0, -14);

            nameRT.anchoredPosition = new Vector2(60, -4);
            effRT.anchoredPosition = new Vector2(60, -22);
        }
        else
        {
            imgRT.sizeDelta = Vector2.zero;

            nameRT.anchoredPosition = new Vector2(4, -2);
            effRT.anchoredPosition = new Vector2(4, -14);
        }
    }

    private void RepositionDiceRows()
    {
        float y = 0f;

        for (int i = 0; i < diceBlock.childCount; i++)
        {
            RectTransform rt = diceBlock.GetChild(i).GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(0, -y);
            y += rt.rect.height + 4f;
        }
    }

    private void RepositionSummary()
    {
        float totalHeight = 0f;

        for (int i = 0; i < diceBlock.childCount; i++)
        {
            RectTransform rt = diceBlock.GetChild(i).GetComponent<RectTransform>();
            totalHeight += rt.rect.height + 4f;
        }

        summaryBlock.anchoredPosition = new Vector2(0, -totalHeight - 4f);
    }
}
