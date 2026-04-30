using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatsUI : MonoBehaviour
{
    [Header("Prefab (DiceResultRow)")]
    [SerializeField] private GameObject rowPrefab;

    [Header("Root (usa StatText como contenedor)")]
    [SerializeField] private RectTransform statsRoot;

    [Header("Icons")]
    [SerializeField] private Sprite rollsIcon;
    [SerializeField] private Sprite goldIcon;
    [SerializeField] private Sprite lapsIcon;
    [SerializeField] private Sprite rerollsIcon;

    [Header("Ajustes visuales")]
    [SerializeField] private float rowHeight = 40f;
    [SerializeField] private float rowSpacing = 2f;
    [SerializeField] private int fontSize = 40;

    [Header("Layout interno")]
    [SerializeField] private float rowWidth = 650f;
    [SerializeField] private float iconSize = 48f;
    [SerializeField] private float nameOffsetX = 40f;
    [SerializeField] private float valueOffsetX = 225f;
    [SerializeField] private float nameWidth = 150f;

    [Header("Animación")]
    [SerializeField] private float coinAnimationSpeed = 0.25f;

    private void OnEnable()
    {
        StartCoroutine(DelayedInit());
    }

    private IEnumerator DelayedInit()
    {
        yield return null;
        yield return null;

        ForceZeroOffsets();

        if (StatManager.Instance != null)
            StatManager.Instance.OnStatsChanged += RefreshUI;

        RefreshUI();
    }

    private void OnDisable()
    {
        if (StatManager.Instance != null)
            StatManager.Instance.OnStatsChanged -= RefreshUI;
    }

    private void ForceZeroOffsets()
    {
        if (statsRoot == null)
            return;

        statsRoot.offsetMin = Vector2.zero;
        statsRoot.offsetMax = Vector2.zero;
    }

    private void RefreshUI()
    {
        if (statsRoot == null || rowPrefab == null || StatManager.Instance == null)
            return;

        for (int i = statsRoot.childCount - 1; i >= 0; i--)
            Destroy(statsRoot.GetChild(i).gameObject);

        var sm = StatManager.Instance;
        float currentY = 0f;

        CreateRow("Tiradas", rollsIcon, sm.GetCurrentValue(StatType.Rolls).ToString(), ref currentY);

        int gold = sm.GetCurrentValue(StatType.Gold);
        int maxGold = sm.GetMaxValue(StatType.Gold);
        CreateRow("Pesetas", goldIcon, gold + "/" + maxGold, ref currentY);

        Movement player = FindFirstObjectByType<Movement>();
        if (player != null && player.isPlayer)
        {
            int laps = player.Round - 1;
            CreateRow("Vueltas", lapsIcon, laps.ToString(), ref currentY);
        }

        if (sm.IsPlayerInShop())
        {
            int rerolls = sm.GetCurrentValue(StatType.ShopRerolls);
            int maxRerolls = sm.GetMaxValue(StatType.ShopRerolls);
            CreateRow("Rerolls", rerollsIcon, rerolls + "/" + maxRerolls, ref currentY);
        }
    }

    private void CreateRow(string name, Sprite icon, string value, ref float currentY)
    {
        var rowObj = Instantiate(rowPrefab);
        rowObj.transform.SetParent(statsRoot, false);
        rowObj.transform.localScale = Vector3.one;

        var nameTMP = rowObj.transform.Find("NameText").GetComponent<TextMeshProUGUI>();
        var img = rowObj.transform.Find("Image").GetComponent<Image>();
        var effTMP = rowObj.transform.Find("EffectsText").GetComponent<TextMeshProUGUI>();
        var imgRT = img.rectTransform;

        effTMP.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 170f);

        nameTMP.fontSize = fontSize;
        effTMP.fontSize = fontSize;

        nameTMP.text = name + ":";
        effTMP.text = value;

        nameTMP.lineSpacing = 0f;
        effTMP.lineSpacing = 0f;

        if (name == "Pesetas")
        {
            if (!img.TryGetComponent<Animator>(out Animator anim))
                anim = img.gameObject.AddComponent<Animator>();

            anim.runtimeAnimatorController =
                Resources.Load<RuntimeAnimatorController>("Sprites/Animations/Moneda bien");

            anim.speed = coinAnimationSpeed;
            img.enabled = true;
        }
        else
        {
            if (img.TryGetComponent<Animator>(out var anim))
                Destroy(anim);

            img.sprite = icon;
            img.enabled = icon != null;
        }

        // IMPORTANTE: preserveAspect en runtime
        img.preserveAspect = true;

        // Tamaño máximo, respetando proporción real del sprite
        imgRT.sizeDelta = new Vector2(iconSize, iconSize);

        var rt = rowObj.GetComponent<RectTransform>();

        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);

        rt.sizeDelta = new Vector2(rowWidth, rowHeight);
        rt.anchoredPosition = new Vector2(0f, currentY);

        PositionRow(nameTMP.rectTransform, effTMP.rectTransform, imgRT);

        nameTMP.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, nameWidth);

        currentY -= (rowHeight + rowSpacing);
    }

    private void PositionRow(RectTransform nameRT, RectTransform effRT, RectTransform imgRT)
    {
        imgRT.anchorMin = imgRT.anchorMax = imgRT.pivot = new Vector2(0f, 1f);
        nameRT.anchorMin = nameRT.anchorMax = nameRT.pivot = new Vector2(0f, 1f);
        effRT.anchorMin = effRT.anchorMax = effRT.pivot = new Vector2(0f, 1f);

        imgRT.anchoredPosition = new Vector2(0f, -2f);
        nameRT.anchoredPosition = new Vector2(nameOffsetX, -2f);
        effRT.anchoredPosition = new Vector2(valueOffsetX, -2f);
    }
}
