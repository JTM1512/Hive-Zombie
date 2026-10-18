using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameHud : MonoBehaviour
{
    readonly List<AbilityRow> rows = new List<AbilityRow>();
    TextMeshProUGUI zombieCountText;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void RegisterSceneHook()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void EnsureHud()
    {
        TryCreateHud();
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        TryCreateHud();
    }

    static void TryCreateHud()
    {
        PlayerMovement player = FindAnyObjectByType<PlayerMovement>();
        if (player == null) return;

        EnsureAbilities(player.gameObject);

        if (FindAnyObjectByType<GameHud>() != null) return;

        GameObject hudObject = new GameObject("Game Hud");
        hudObject.AddComponent<GameHud>();
    }

    static void EnsureAbilities(GameObject player)
    {
        if (player.GetComponent<Horde>() == null)
            player.AddComponent<Horde>();
        if (player.GetComponent<ShieldAbility>() == null)
            player.AddComponent<ShieldAbility>();
        if (player.GetComponent<SacrificeAbility>() == null)
            player.AddComponent<SacrificeAbility>();
        if (player.GetComponent<AttackAbility>() == null)
            player.AddComponent<AttackAbility>();
    }

    void Start()
    {
        BuildCanvas();
        if (Tracker.Instance != null)
            Tracker.Instance.CountChanged += OnCountChanged;
        Refresh();
    }

    void OnDestroy()
    {
        if (Tracker.Instance != null)
            Tracker.Instance.CountChanged -= OnCountChanged;
    }

    void OnCountChanged(int count)
    {
        Refresh();
    }

    void BuildCanvas()
    {
        GameObject canvasObject = new GameObject("Hud Canvas", typeof(RectTransform));
        canvasObject.transform.SetParent(transform, false);

        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 5;
        canvas.additionalShaderChannels =
            AdditionalCanvasShaderChannels.TexCoord1 |
            AdditionalCanvasShaderChannels.Normal |
            AdditionalCanvasShaderChannels.Tangent;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        canvasObject.AddComponent<GraphicRaycaster>();

        float abilitiesHeight = CreateAbilitiesPanel(canvasObject.transform);
        zombieCountText = CreatePanel(
            canvasObject.transform,
            "Zombie Count",
            new Vector2(1f, 1f),
            new Vector2(1f, 1f),
            new Vector2(-28f, -28f - abilitiesHeight - 12f),
            new Vector2(1f, 1f),
            new Vector2(420f, 156f),
            34,
            TextAlignmentOptions.MidlineLeft);
        zombieCountText.textWrappingMode = TextWrappingModes.Normal;
        zombieCountText.lineSpacing = 6f;
    }

    float CreateAbilitiesPanel(Transform parent)
    {
        const float rowHeight = 48f;
        GameObject panel = new GameObject("Abilities", typeof(RectTransform));
        panel.transform.SetParent(parent, false);

        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(1f, 1f);
        rect.anchoredPosition = new Vector2(-28f, -28f);
        rect.sizeDelta = new Vector2(420f, 220f);

        Image background = panel.AddComponent<Image>();
        background.color = new Color(0.04f, 0.06f, 0.04f, 0.92f);
        background.raycastTarget = false;

        CreateLabel(panel.transform, "ABILITIES", 26, FontStyles.Bold, new Color(0.86f, 0.96f, 0.62f, 1f),
            new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -12f), new Vector2(-36f, 36f), TextAlignmentOptions.MidlineLeft);

        IAbilityInfo[] abilities = FindAbilities();
        if (abilities.Length == 0)
        {
            TextMeshProUGUI empty = CreateLabel(panel.transform, "None available", 30, FontStyles.Bold,
                Color.white,
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -56f), new Vector2(-36f, 42f), TextAlignmentOptions.MidlineLeft);
            rows.Add(new AbilityRow(null, empty));
            rect.sizeDelta = new Vector2(420f, 112f);
            return 112f;
        }

        for (int i = 0; i < abilities.Length; i++)
        {
            TextMeshProUGUI line = CreateLabel(panel.transform, "", 32, FontStyles.Bold, Color.white,
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -56f - (i * rowHeight)), new Vector2(-36f, 44f), TextAlignmentOptions.MidlineLeft);
            rows.Add(new AbilityRow(abilities[i], line));
        }

        float height = 68f + (abilities.Length * rowHeight);
        rect.sizeDelta = new Vector2(420f, height);
        return height;
    }

    static IAbilityInfo[] FindAbilities()
    {
        List<IAbilityInfo> found = new List<IAbilityInfo>();
        MonoBehaviour[] behaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour is IAbilityInfo ability && !found.Contains(ability))
                found.Add(ability);
        }

        found.Sort((a, b) => string.CompareOrdinal(a.KeyLabel, b.KeyLabel));
        return found.ToArray();
    }

    TextMeshProUGUI CreatePanel(
        Transform parent,
        string name,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 position,
        Vector2 pivot,
        Vector2 size,
        float fontSize,
        TextAlignmentOptions alignment)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform));
        panel.transform.SetParent(parent, false);

        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Image background = panel.AddComponent<Image>();
        background.color = new Color(0.04f, 0.06f, 0.04f, 0.92f);
        background.raycastTarget = false;

        return CreateLabel(panel.transform, "Zombies: 1", fontSize, FontStyles.Bold, Color.white,
            Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(-28f, -8f), alignment);
    }

    static TextMeshProUGUI CreateLabel(
        Transform parent,
        string text,
        float fontSize,
        FontStyles style,
        Color color,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 pivot,
        Vector2 position,
        Vector2 size,
        TextAlignmentOptions alignment)
    {
        GameObject labelObject = new GameObject("Label", typeof(RectTransform));
        labelObject.transform.SetParent(parent, false);

        RectTransform rect = labelObject.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        TextMeshProUGUI label = labelObject.AddComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = fontSize;
        label.fontStyle = style;
        label.color = color;
        label.alignment = alignment;
        label.raycastTarget = false;
        label.textWrappingMode = TextWrappingModes.NoWrap;
        label.margin = new Vector4(18f, 2f, 18f, 2f);
        label.outlineWidth = 0.18f;
        label.outlineColor = new Color32(0, 0, 0, 220);
        if (TMP_Settings.defaultFontAsset != null)
            label.font = TMP_Settings.defaultFontAsset;
        return label;
    }

    void Refresh()
    {
        int count = Tracker.Instance != null ? Tracker.Instance.Count : 1;
        int spendable = Mathf.Max(0, count - 1);
        int best = Tracker.Instance != null ? Tracker.Instance.Best : 1;

        if (zombieCountText != null)
            zombieCountText.text = "Zombies: " + count + "\nCan spend: " + spendable + "\nBest: " + best;

        foreach (AbilityRow row in rows)
        {
            if (row.Ability == null) continue;

            int cost = row.Ability.ZombieCost;
            bool ready = spendable >= cost;
            string costColor = ready ? "#D7F09A" : "#C8C8C0";
            row.Label.text = row.Ability.KeyLabel + "   " + row.Ability.DisplayName + ": <color=" + costColor + ">(" + cost + " Cost)</color>";
            row.Label.color = ready ? Color.white : new Color(0.82f, 0.82f, 0.78f, 1f);
        }
    }

    readonly struct AbilityRow
    {
        public AbilityRow(IAbilityInfo ability, TextMeshProUGUI label)
        {
            Ability = ability;
            Label = label;
        }

        public IAbilityInfo Ability { get; }
        public TextMeshProUGUI Label { get; }
    }
}
