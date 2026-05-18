using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class ActiveDiceUI : MonoBehaviour
{
    [Header("UI")]
    public RectTransform inventoryButton;
    public RectTransform resultsRoot;
    public GameObject rowPrefab;

    [Header("Layout")]
    public float extraRight = 6f;
    public float extraDown = 34f;

    private bool isPlayerTurn = true;

    private RectTransform diceBlock;
    private RectTransform summaryBlock;

    private int lastMovement = 0;
    private string lastEffects = "";
    private bool lastWasEnemy = false;
    private bool hasSummary = false;

    private void OnEnable()
    {
        StartCoroutine(DelayedInit());

        TurnManager.OnPlayerTurnStarted += HandlePlayerTurn;
        TurnManager.OnEnemyTurnStarted += HandleEnemyTurn;
        TurnManager.OnEnemyRollCalculated += HandleEnemyRoll;
    }

    private void OnDisable()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnActiveDiceChanged -= RefreshUI;

        TurnManager.OnPlayerTurnStarted -= HandlePlayerTurn;
        TurnManager.OnEnemyTurnStarted -= HandleEnemyTurn;
        TurnManager.OnEnemyRollCalculated -= HandleEnemyRoll;
    }

    private IEnumerator DelayedInit()
    {
        yield return null;

        AlignUnderButton();

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
        rt.sizeDelta = Vector2.zero;

        return rt;
    }

    private void Update()
    {
        if (!Application.isPlaying)
            AlignUnderButton();
    }

    private void AlignUnderButton()
    {
        if (inventoryButton == null || resultsRoot == null)
            return;

        if (resultsRoot.parent != inventoryButton)
            resultsRoot.SetParent(inventoryButton, false);

        resultsRoot.anchorMin = new Vector2(0f, 1f);
        resultsRoot.anchorMax = new Vector2(0f, 1f);
        resultsRoot.pivot = new Vector2(0f, 1f);

        float leftX = -inventoryButton.rect.width * inventoryButton.pivot.x;
        float bottomY = -inventoryButton.rect.height * (1f - inventoryButton.pivot.y);

        resultsRoot.anchoredPosition = new Vector2(
            leftX + extraRight,
            bottomY - extraDown
        );

        resultsRoot.localScale = Vector3.one;
    }

    private void HandlePlayerTurn()
    {
        isPlayerTurn = true;
        RefreshUI();
    }

    private void HandleEnemyTurn()
    {
        isPlayerTurn = false;
        RefreshUI();
    }

    private void HandleEnemyRoll(int total)
    {
        lastWasEnemy = true;
        lastMovement = total;
        lastEffects = "";
        hasSummary = true;          

        RefreshUI();
    }

    // Llamado desde Movement SIEMPRE al final del turno real
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

        // 1. Limpiar dados
        foreach (Transform t in diceBlock)
            Destroy(t.gameObject);

        // 2. Header + dados
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

        // 3. Redibujar resumen SOLO si alguna vez hemos recibido uno
        RedrawSummary();

        // 4. Colocar el bloque de resumen debajo de los dados
        if (diceBlock.childCount > 0)
        {
            RectTransform lastRow = diceBlock.GetChild(diceBlock.childCount - 1).GetComponent<RectTransform>();
            float lastRowBottom = lastRow.anchoredPosition.y - lastRow.rect.height;
            summaryBlock.anchoredPosition = new Vector2(0, lastRowBottom - 20);
        }
        else
        {
            summaryBlock.anchoredPosition = new Vector2(0, -20);
        }
    }

    private void RedrawSummary()
    {
        foreach (Transform t in summaryBlock)
            Destroy(t.gameObject);

        //  Si aún no ha habido ningún turno real, no mostramos nada
        if (!hasSummary)
            return;

        var row = Instantiate(rowPrefab, summaryBlock);

        string title = lastWasEnemy ? "Turno anterior (enemigo)" : "Turno anterior";
        string text = "Mov: " + lastMovement + "  |  Efectos: " +
                      (string.IsNullOrEmpty(lastEffects) ? "ninguno" : lastEffects);

        SetupRow(row, title, text, false);

        var nameTMP = row.transform.Find("NameText").GetComponent<TextMeshProUGUI>();
        nameTMP.rectTransform.sizeDelta = new Vector2(300, nameTMP.rectTransform.sizeDelta.y);
    }

    private void CreateHeader(string text)
    {
        var row = Instantiate(rowPrefab, diceBlock);
        SetupRow(row, text, "", false);
    }

    private void CreateSimpleRow(string text)
    {
        var row = Instantiate(rowPrefab, diceBlock);
        SetupRow(row, "", text, false);
    }

    private void CreateDiceRow(ItemSlot slot)
    {
        var row = Instantiate(rowPrefab, diceBlock);

        var rollInfo = DiceRollManager.Instance.GetRollInfo(slot);

        if (!rollInfo.HasValue)
        {
            SetupRow(row, "", "Sin tirar", false);
            return;
        }

        DiceSO dice = slot.ItemSO as DiceSO;
        if (dice == null)
        {
            SetupRow(row, "", "Error: dado no encontrado", false);
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

        SetupRow(row, "", effText, true);
    }

    private void SetupRow(GameObject row, string nameText, string effectsText, bool rolled)
    {
        var nameTMP = row.transform.Find("NameText").GetComponent<TextMeshProUGUI>();
        var effTMP = row.transform.Find("EffectsText").GetComponent<TextMeshProUGUI>();
        var img = row.transform.Find("Image").GetComponent<Image>();

        nameTMP.text = nameText;
        effTMP.text = effectsText;

        nameTMP.lineSpacing = -15f;
        effTMP.lineSpacing = -15f;

        var rt = row.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);

        rt.sizeDelta = new Vector2(350, rolled ? 90 : 40);

        PositionRow(nameTMP.rectTransform, effTMP.rectTransform, img.rectTransform, rolled);
    }

    private void PositionRow(RectTransform nameRT, RectTransform effRT, RectTransform imgRT, bool rolled)
    {
        imgRT.anchorMin = imgRT.anchorMax = imgRT.pivot = new Vector2(0f, 1f);
        nameRT.anchorMin = nameRT.anchorMax = nameRT.pivot = new Vector2(0f, 1f);
        effRT.anchorMin = effRT.anchorMax = effRT.pivot = new Vector2(0f, 1f);

        if (rolled)
        {
            imgRT.sizeDelta = new Vector2(50, 50);
            imgRT.anchoredPosition = new Vector2(0, -26);

            nameRT.anchoredPosition = new Vector2(60, -10);
            effRT.anchoredPosition = new Vector2(60, -42);
        }
        else
        {
            imgRT.sizeDelta = Vector2.zero;

            nameRT.anchoredPosition = new Vector2(0, -2);
            effRT.anchoredPosition = new Vector2(0, -22);
        }
    }
}
