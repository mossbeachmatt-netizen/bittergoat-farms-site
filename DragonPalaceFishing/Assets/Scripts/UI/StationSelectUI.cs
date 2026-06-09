using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Station selection screen — fully input-system independent.
/// All clicks detected via Input.GetMouseButtonDown for reliability.
/// </summary>
public class StationSelectUI : MonoBehaviour
{
    // ─── Inspector Fields ──────────────────────────────────────────────────────

    [Header("Station Select Panel")]
    public GameObject SelectPanel;

    [Header("Station Buttons (0-5)")]
    public RectTransform[] StationButtons = new RectTransform[6];

    [Header("Station Sprites — assign in order: BL, BC, BR, TL, TC, TR")]
    public Sprite[] StationSprites = new Sprite[6];

    [Header("AI Count Controls")]
    public RectTransform AIMinusButton;
    public RectTransform AIPlusButton;
    public TextMeshProUGUI AICountLabel;

    [Header("Start Button")]
    public RectTransform StartButton;

    [Header("Remove these - set to none")]
    public Button OldPlayButton;

    // ─── Visual settings ───────────────────────────────────────────────────────
    private static readonly Color SelectedTint = new Color(1.0f, 1.0f, 1.0f, 1.0f);
    private static readonly Color OccupiedTint = new Color(0.75f, 0.75f, 0.75f, 1.0f);
    private static readonly Color EmptyTint    = new Color(0.3f,  0.3f,  0.3f,  0.5f);

    private const float SelectedScale = 1.15f;
    private const float NormalScale   = 1.0f;
    private const float ScaleSpeed    = 8.0f;
    private const float TextBelowGap  = 4f;

    // ─── Button positions ──────────────────────────────────────────────────────
    private static readonly Vector2[] SlotPositions = new Vector2[]
    {
        new Vector2(-500f, -300f), // BL
        new Vector2(   0f, -300f), // BC
        new Vector2( 500f, -300f), // BR
        new Vector2(-500f,  300f), // TL
        new Vector2(   0f,  300f), // TC
        new Vector2( 500f,  300f), // TR
    };

    // ─── AI removal order ──────────────────────────────────────────────────────
    private static readonly int[] AIRemovalOrder = new int[] { 5, 4, 3, 2, 0, 1 };

    private static readonly string[] SlotLabels = new string[]
    {
        "Bottom Left", "Bottom Center", "Bottom Right",
        "Top Left",    "Top Center",    "Top Right"
    };

    // ─── Runtime State ─────────────────────────────────────────────────────────
    private int _selectedSlot = 1;
    private int _aiCount      = 5;

    // ─── Cached references ─────────────────────────────────────────────────────
    private Image[]           _buttonImages;
    private TextMeshProUGUI[] _labels;
    private Vector3[]         _targetScales;

    // ─── Unity Lifecycle ───────────────────────────────────────────────────────

    private void Start()
    {
        if (OldPlayButton != null)
            OldPlayButton.gameObject.SetActive(false);

        CacheAndSetupButtons();

        GameState.Instance.OnPhaseChanged += OnPhaseChanged;
        RefreshButtons();
        RefreshAILabel();
    }

    private void OnDestroy()
    {
        if (GameState.Instance != null)
            GameState.Instance.OnPhaseChanged -= OnPhaseChanged;
    }

    private void Update()
    {
        // Animate button scales
        for (int i = 0; i < StationButtons.Length; i++)
        {
            if (StationButtons[i] == null) continue;
            StationButtons[i].localScale = Vector3.Lerp(
                StationButtons[i].localScale,
                _targetScales[i],
                Time.deltaTime * ScaleSpeed);
        }

        if (!Input.GetMouseButtonDown(0)) return;
        if (SelectPanel == null || !SelectPanel.activeSelf) return;

        Vector2 mousePos = Input.mousePosition;

        for (int i = 0; i < StationButtons.Length; i++)
            if (StationButtons[i] != null && IsMouseOver(StationButtons[i], mousePos))
            { OnStationClicked(i); return; }

        if (AIMinusButton != null && IsMouseOver(AIMinusButton, mousePos)) { OnAIMinus();      return; }
        if (AIPlusButton  != null && IsMouseOver(AIPlusButton,  mousePos)) { OnAIPlus();       return; }
        if (StartButton   != null && IsMouseOver(StartButton,   mousePos)) { OnStartClicked(); return; }
    }

    // ─── Setup ─────────────────────────────────────────────────────────────────

    private void CacheAndSetupButtons()
    {
        int count     = StationButtons.Length;
        _buttonImages = new Image[count];
        _labels       = new TextMeshProUGUI[count];
        _targetScales = new Vector3[count];

        for (int i = 0; i < count; i++)
        {
            if (StationButtons[i] == null) continue;

            // ── Reposition button ──
            StationButtons[i].anchoredPosition = SlotPositions[i];

            // ── Find art Image via Button targetGraphic ──
            Button btn = StationButtons[i].GetComponent<Button>();
            _buttonImages[i] = (btn != null && btn.targetGraphic is Image tg)
                ? tg
                : StationButtons[i].GetComponent<Image>();

            // ── Assign sprite ──
            if (_buttonImages[i] != null && StationSprites != null &&
                i < StationSprites.Length && StationSprites[i] != null)
            {
                _buttonImages[i].sprite         = StationSprites[i];
                _buttonImages[i].type           = Image.Type.Simple;
                _buttonImages[i].preserveAspect = true;
            }

            // ── Make button square ──
            float w = StationButtons[i].rect.width;
            if (w > 1f)
                StationButtons[i].SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, w);

            // ── Initial scale ──
            _targetScales[i] = Vector3.one * NormalScale;

            // ── Reposition label below button ──
            _labels[i] = StationButtons[i].GetComponentInChildren<TextMeshProUGUI>();
            if (_labels[i] != null)
            {
                RectTransform labelRT    = _labels[i].GetComponent<RectTransform>();
                labelRT.anchorMin        = new Vector2(0.5f, 0f);
                labelRT.anchorMax        = new Vector2(0.5f, 0f);
                labelRT.pivot            = new Vector2(0.5f, 1f);
                labelRT.anchoredPosition = new Vector2(0f, -TextBelowGap);
                labelRT.sizeDelta        = new Vector2(160f, 50f);
                _labels[i].alignment     = TextAlignmentOptions.Center;
            }
        }
    }

    // ─── Hit Testing ───────────────────────────────────────────────────────────

    private bool IsMouseOver(RectTransform rt, Vector2 mousePos)
        => RectTransformUtility.RectangleContainsScreenPoint(rt, mousePos, null);

    // ─── Phase Handling ────────────────────────────────────────────────────────

    private void OnPhaseChanged(GameState.Phase phase)
    {
        bool show = (phase == GameState.Phase.Options);
        if (SelectPanel != null) SelectPanel.SetActive(show);
    }

    // ─── Button Handlers ───────────────────────────────────────────────────────

    private void OnStationClicked(int slotIndex)
    {
        _selectedSlot = slotIndex;
        RefreshButtons();
        Debug.Log($"[StationSelectUI] Selected slot {slotIndex} ({SlotLabels[slotIndex]})");
    }

    private void OnStartClicked()
    {
        StationManager sm = FindAnyObjectByType<StationManager>();
        if (sm != null)
        {
            sm.PlayerSlotIndex = _selectedSlot;
            sm.AICount         = _aiCount;
        }
        GameState.Instance.StartRound();
    }

    private void OnAIMinus()
    {
        _aiCount = Mathf.Max(0, _aiCount - 1);
        RefreshButtons();
        RefreshAILabel();
    }

    private void OnAIPlus()
    {
        _aiCount = Mathf.Min(5, _aiCount + 1);
        RefreshButtons();
        RefreshAILabel();
    }

    // ─── UI Refresh ────────────────────────────────────────────────────────────

    private void RefreshButtons()
    {
        bool[] isAI = new bool[StationButtons.Length];
        int filled = 0;
        for (int r = AIRemovalOrder.Length - 1; r >= 0 && filled < _aiCount; r--)
        {
            int slot = AIRemovalOrder[r];
            if (slot == _selectedSlot) continue;
            isAI[slot] = true;
            filled++;
        }

        for (int i = 0; i < StationButtons.Length; i++)
        {
            if (StationButtons[i] == null) continue;
            bool selected = (i == _selectedSlot);

            if (_buttonImages[i] != null)
                _buttonImages[i].color = selected ? SelectedTint
                                       : isAI[i]  ? OccupiedTint
                                                  : EmptyTint;

            _targetScales[i] = Vector3.one * (selected ? SelectedScale : NormalScale);

            if (_labels[i] != null)
                _labels[i].text = selected ? $"{SlotLabels[i]}\nYOU"
                                : isAI[i]  ? $"{SlotLabels[i]}\nAI"
                                           : SlotLabels[i];
        }
    }

    private void RefreshAILabel()
    {
        if (AICountLabel != null)
            AICountLabel.text = $"AI: {_aiCount}";
    }

    // ─── Public ────────────────────────────────────────────────────────────────

    public void ShowSelectionScreen()
    {
        GameState.Instance.SetPhase(GameState.Phase.Options);
        if (SelectPanel != null) SelectPanel.SetActive(true);
        RefreshButtons();
        RefreshAILabel();
    }
}