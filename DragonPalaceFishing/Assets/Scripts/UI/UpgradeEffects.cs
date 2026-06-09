using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Handles visual feedback for upgrades:
/// - Floating popup text when upgrade triggers
/// - Timer bars above each station showing upgrade duration
/// Attach to a dedicated _UpgradeEffects GameObject in the scene.
/// </summary>
public class UpgradeEffects : MonoBehaviour
{
    // ─── Inspector Fields ──────────────────────────────────────────────────────

    [Header("Popup Settings")]
    [Tooltip("Prefab with TextMeshProUGUI — simple text object")]
    public GameObject PopupPrefab;

    [Tooltip("Canvas to spawn popups on")]
    public Canvas GameCanvas;

    [Header("Timer Bar Settings")]
    [Tooltip("Prefab with an Image component for the timer bar")]
    public GameObject TimerBarPrefab;

    // ─── Runtime State ─────────────────────────────────────────────────────────

    // One timer bar per station (index matches Stations list)
    private List<Image>      _timerBars     = new List<Image>();
    private List<GameObject> _timerBarRoots = new List<GameObject>();

    // Track previous upgrade state to detect changes
    private List<string> _prevUpgrades = new List<string>();

    // ─── Upgrade Colors ────────────────────────────────────────────────────────

    private static readonly Dictionary<string, Color> UpgradeColors = new Dictionary<string, Color>
    {
        { "double", new Color(0.0f,  1.0f,  0.8f,  1f) },  // cyan
        { "laser",  new Color(0.0f,  0.8f,  1.0f,  1f) },  // blue-cyan
        { "triple", new Color(1.0f,  0.84f, 0.0f,  1f) },  // gold
    };

    private static readonly Dictionary<string, string> UpgradeLabels = new Dictionary<string, string>
    {
        { "double", "DOUBLE BARREL!" },
        { "laser",  "LASER MODE!"    },
        { "triple", "TRIPLE CANNON!" },
    };

    // ─── Unity Lifecycle ───────────────────────────────────────────────────────

    private void Start()
    {
        GameState.Instance.OnPhaseChanged += OnPhaseChanged;
    }

    private void OnDestroy()
    {
        if (GameState.Instance != null)
            GameState.Instance.OnPhaseChanged -= OnPhaseChanged;
    }

    private void Update()
    {
        if (GameState.Instance.CurrentPhase != GameState.Phase.Playing) return;

        var stations = GameState.Instance.Stations;

        // Initialise tracking lists if needed
        while (_prevUpgrades.Count < stations.Count) _prevUpgrades.Add(null);
        while (_timerBars.Count   < stations.Count) _timerBars.Add(null);
        while (_timerBarRoots.Count < stations.Count) _timerBarRoots.Add(null);

        for (int i = 0; i < stations.Count; i++)
        {
            StationData st = stations[i];

            // Detect new upgrade
            if (st.ActiveUpgrade != _prevUpgrades[i])
            {
                if (st.ActiveUpgrade != null)
                {
                    SpawnPopup(st);
                    SpawnTimerBar(i, st);
                }
                else
                {
                    // Upgrade expired — remove timer bar
                    RemoveTimerBar(i);
                }
                _prevUpgrades[i] = st.ActiveUpgrade;
            }

            // Update timer bar fill
            if (_timerBars[i] != null && st.ActiveUpgrade != null)
            {
                float maxDuration = FindAnyObjectByType<StationManager>()?.UpgradeDuration ?? 8f;
                _timerBars[i].fillAmount = st.UpgradeTimer / maxDuration;
            }
        }
    }

    // ─── Popup ─────────────────────────────────────────────────────────────────

    private void SpawnPopup(StationData st)
    {
        if (PopupPrefab == null || GameCanvas == null) return;

        string label = UpgradeLabels.ContainsKey(st.ActiveUpgrade)
            ? UpgradeLabels[st.ActiveUpgrade] : st.ActiveUpgrade.ToUpper() + "!";

        Color col = UpgradeColors.ContainsKey(st.ActiveUpgrade)
            ? UpgradeColors[st.ActiveUpgrade] : Color.white;

        // Convert station world position to canvas position
        Vector2 screenPos = Camera.main.WorldToScreenPoint(st.Position);
        Vector2 canvasPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            GameCanvas.GetComponent<RectTransform>(),
            screenPos, GameCanvas.worldCamera, out canvasPos);

        GameObject go = Instantiate(PopupPrefab, GameCanvas.transform);
        go.GetComponent<RectTransform>().anchoredPosition = canvasPos;

        TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();
        if (tmp != null)
        {
            tmp.text      = label;
            tmp.color     = col;
            tmp.fontSize  = 32;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
        }

        StartCoroutine(AnimatePopup(go));
    }

    private IEnumerator AnimatePopup(GameObject go)
    {
        if (go == null) yield break;

        RectTransform rt  = go.GetComponent<RectTransform>();
        TextMeshProUGUI t = go.GetComponent<TextMeshProUGUI>();
        float duration    = 2f;
        float elapsed     = 0f;
        Vector2 startPos  = rt.anchoredPosition;

        while (elapsed < duration && go != null)
        {
            elapsed += Time.deltaTime;
            float pct = elapsed / duration;

            // Float upward
            rt.anchoredPosition = startPos + Vector2.up * (80f * pct);

            // Fade out in second half
            if (t != null)
            {
                Color c = t.color;
                c.a     = pct < 0.5f ? 1f : 1f - ((pct - 0.5f) / 0.5f);
                t.color = c;
            }

            yield return null;
        }

        if (go != null) Destroy(go);
    }

    // ─── Timer Bar ─────────────────────────────────────────────────────────────

    private void SpawnTimerBar(int index, StationData st)
    {
        // Remove any existing bar first
        RemoveTimerBar(index);

        if (TimerBarPrefab == null || GameCanvas == null) return;

        Color col = UpgradeColors.ContainsKey(st.ActiveUpgrade)
            ? UpgradeColors[st.ActiveUpgrade] : Color.white;

        // Convert station world position to canvas position
        Vector2 screenPos = Camera.main.WorldToScreenPoint(st.Position);
        Vector2 canvasPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            GameCanvas.GetComponent<RectTransform>(),
            screenPos, GameCanvas.worldCamera, out canvasPos);

        GameObject go = Instantiate(TimerBarPrefab, GameCanvas.transform);
        RectTransform rt = go.GetComponent<RectTransform>();

        // Position above the station
        rt.anchoredPosition = canvasPos + Vector2.up * 60f;
        rt.sizeDelta        = new Vector2(100f, 12f);

        Image img = go.GetComponent<Image>();
        if (img != null)
        {
            img.color     = col;
            img.type      = Image.Type.Filled;
            img.fillMethod = Image.FillMethod.Horizontal;
            img.fillAmount = 1f;
        }

        _timerBars[index]     = img;
        _timerBarRoots[index] = go;
    }

    private void RemoveTimerBar(int index)
    {
        if (index < _timerBarRoots.Count && _timerBarRoots[index] != null)
        {
            Destroy(_timerBarRoots[index]);
            _timerBarRoots[index] = null;
            _timerBars[index]     = null;
        }
    }

    // ─── Phase Changes ─────────────────────────────────────────────────────────

    private void OnPhaseChanged(GameState.Phase phase)
    {
        if (phase == GameState.Phase.Countdown)
        {
            // Clear all bars and tracking on new round
            for (int i = 0; i < _timerBarRoots.Count; i++)
                RemoveTimerBar(i);

            _timerBars.Clear();
            _timerBarRoots.Clear();
            _prevUpgrades.Clear();
        }
    }
}