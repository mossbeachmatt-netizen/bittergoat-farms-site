using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

/// <summary>
/// Manages all UI panels including the new splash/title screen.
/// Splash screen shows on startup — any key advances to station select.
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("Splash")]
    public GameObject TitleScreenSprite;  // world space sprite — sharp rendering
    public GameObject SplashUIPanel;      // canvas panel for Press Any Key text only

    [Header("Panels")]
    public GameObject TitlePanel;
    public GameObject GamePanel;
    public GameObject RoundEndPanel;

    [Header("Splash Panel")]
    public TextMeshProUGUI PressAnyKeyLabel;

    [Header("Title Panel")]
    public Button PlayButton;

    [Header("Game Panel")]
    public TextMeshProUGUI CountdownLabel;
    public TextMeshProUGUI TimerLabel;
    public TextMeshProUGUI[] ScoreLabels = new TextMeshProUGUI[6];

    [Header("Round End Panel")]
    public TextMeshProUGUI RoundEndTitle;
    public TextMeshProUGUI RoundEndScores;
    public Button          PlayAgainButton;

    [Header("Score Label Offset")]
    public Vector2 ScoreLabelOffset = new Vector2(0f, 80f);

    // ─── State ─────────────────────────────────────────────────────────────────
    private bool _onSplashScreen = true;
    private float _splashTimer   = 0f;
    
    // ─── Unity Lifecycle ───────────────────────────────────────────────────────

    private void Start()
    {
        if (PlayButton      != null) PlayButton.onClick.AddListener(OnPlayClicked);
        if (PlayAgainButton != null) PlayAgainButton.onClick.AddListener(OnPlayAgainClicked);

        GameState.Instance.OnPhaseChanged += OnPhaseChanged;

        // Show splash screen first — hide everything else
        ShowSplash();
    }

    private void OnDestroy()
    {
        if (GameState.Instance != null)
            GameState.Instance.OnPhaseChanged -= OnPhaseChanged;
    }

    private void Update()
    {
        if (_onSplashScreen)
        {
            UpdateSplash();
            return;
        }

        // Spacebar starts or restarts
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (GameState.Instance.CurrentPhase == GameState.Phase.Title ||
                GameState.Instance.CurrentPhase == GameState.Phase.Lobby)
                OnPlayClicked();
            else if (GameState.Instance.CurrentPhase == GameState.Phase.RoundEnd)
                OnPlayAgainClicked();
        }

        if (GameState.Instance.CurrentPhase == GameState.Phase.Countdown)
            UpdateCountdown();
        else if (GameState.Instance.CurrentPhase == GameState.Phase.Playing)
            UpdateHUD();
    }

    // ─── Splash Screen ─────────────────────────────────────────────────────────

    private void ShowSplash()
    {
        _onSplashScreen = true;
        SetActive(TitleScreenSprite, true);
        SetActive(SplashUIPanel,     true);
        SetActive(TitlePanel,        false);
        SetActive(GamePanel,         false);
        SetActive(RoundEndPanel,     false);
        Debug.Log($"[UIManager] ShowSplash called");
    }

    private void UpdateSplash()
    {
        // Pulse the "Press Any Key" label
        _splashTimer += Time.deltaTime;
        if (PressAnyKeyLabel != null)
        {
            float alpha = Mathf.Abs(Mathf.Sin(_splashTimer * 1.5f));
            Color c     = PressAnyKeyLabel.color;
            c.a         = Mathf.Clamp(alpha, 0.3f, 1f);
            PressAnyKeyLabel.color = c;
        }

        // Any key or mouse click advances to station select
        bool anyKey = Input.anyKeyDown ||
                      (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame);

        if (anyKey)
        {
            _onSplashScreen = false;
            SetActive(TitleScreenSprite, false);
            SetActive(SplashUIPanel,     false);
            OnPlayClicked();
        }
    }

    // ─── Phase Display ─────────────────────────────────────────────────────────

    private void OnPhaseChanged(GameState.Phase phase)
    {
        if (_onSplashScreen) return;

        RefreshPanels(phase);

        if (phase == GameState.Phase.RoundEnd)
            ShowRoundEnd();

        if (phase == GameState.Phase.Playing || phase == GameState.Phase.Countdown)
            PositionScoreLabels();
    }

    private void RefreshPanels(GameState.Phase phase)
    {
        SetActive(TitleScreenSprite, false);
        SetActive(SplashUIPanel,     false);
        SetActive(TitlePanel,    false);
        SetActive(GamePanel,     false);
        SetActive(RoundEndPanel, false);

        switch (phase)
        {
            case GameState.Phase.Title:
            case GameState.Phase.Lobby:
                SetActive(TitlePanel, true);
                break;

            case GameState.Phase.Options:
                break;

            case GameState.Phase.Countdown:
            case GameState.Phase.Playing:
                SetActive(GamePanel, true);
                break;

            case GameState.Phase.RoundEnd:
                SetActive(GamePanel,     true);
                SetActive(RoundEndPanel, true);
                break;
        }
    }

    // ─── Score Label Positioning ───────────────────────────────────────────────

    private void PositionScoreLabels()
    {
        var stations = GameState.Instance.Stations;
        if (stations == null || stations.Count == 0) return;

        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null) return;

        RectTransform canvasRT = canvas.GetComponent<RectTransform>();

        for (int i = 0; i < ScoreLabels.Length; i++)
        {
            if (ScoreLabels[i] == null) continue;
            if (i >= stations.Count)
            {
                ScoreLabels[i].gameObject.SetActive(false);
                continue;
            }

            StationData st        = stations[i];
            Vector2     screenPos = Camera.main.WorldToScreenPoint(st.Position);
            Vector2     canvasPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRT, screenPos, canvas.worldCamera, out canvasPos);

            Vector2 offset = st.Position.y > 0
                ? new Vector2(ScoreLabelOffset.x, -Mathf.Abs(ScoreLabelOffset.y))
                : new Vector2(ScoreLabelOffset.x,  Mathf.Abs(ScoreLabelOffset.y));

            ScoreLabels[i].GetComponent<RectTransform>().anchoredPosition = canvasPos + offset;
            ScoreLabels[i].gameObject.SetActive(true);
        }
    }

    // ─── Countdown ─────────────────────────────────────────────────────────────

    private void UpdateCountdown()
    {
        if (CountdownLabel == null) return;
        CountdownLabel.gameObject.SetActive(true);
        int val = GameState.Instance.CountdownVal;
        CountdownLabel.text = val > 0 ? val.ToString() : "GO!";
    }

    // ─── HUD ───────────────────────────────────────────────────────────────────

    private void UpdateHUD()
    {
        if (CountdownLabel != null)
            CountdownLabel.gameObject.SetActive(false);

        if (TimerLabel != null)
        {
            int seconds = Mathf.CeilToInt(GameState.Instance.TimeLeft);
            TimerLabel.text = $"TIME  {seconds:00}";
        }

        var stations = GameState.Instance.Stations;
        for (int i = 0; i < ScoreLabels.Length; i++)
        {
            if (ScoreLabels[i] == null) continue;
            if (i < stations.Count)
            {
                StationData st       = stations[i];
                string tag           = st.IsPlayer ? ">> " : "";
                ScoreLabels[i].text  = $"{tag}{st.Label}\n{st.Score:N0}";
                ScoreLabels[i].color = st.Color;
            }
            else
            {
                ScoreLabels[i].text = "";
            }
        }
    }

    // ─── Round End ─────────────────────────────────────────────────────────────

    private void ShowRoundEnd()
    {
        var stations = GameState.Instance.Stations;
        if (stations == null || stations.Count == 0) return;

        StationData winner = stations[0];
        foreach (var st in stations)
            if (st.Score > winner.Score) winner = st;

        if (RoundEndTitle != null)
            RoundEndTitle.text = winner.IsPlayer ? "YOU WIN!" : $"{winner.Label} WINS!";

        var sorted = new List<StationData>(stations);
        sorted.Sort((a, b) => b.Score.CompareTo(a.Score));

        if (RoundEndScores != null)
        {
            var sb = new System.Text.StringBuilder();
            foreach (var st in sorted)
                sb.AppendLine($"{st.Label}   {st.Score:N0}");
            RoundEndScores.text = sb.ToString();
        }
    }

    // ─── Button Handlers ───────────────────────────────────────────────────────

    public void OnPlayClicked()
    {
        StationSelectUI select = FindAnyObjectByType<StationSelectUI>();
        if (select != null)
            select.ShowSelectionScreen();
        else
            GameState.Instance.StartRound();
    }

    public void OnPlayAgainClicked()
    {
        StationSelectUI select = FindAnyObjectByType<StationSelectUI>();
        if (select != null)
            select.ShowSelectionScreen();
        else
            GameState.Instance.StartRound();
    }

    private void SetActive(GameObject go, bool active)
    {
        if (go != null) go.SetActive(active);
    }
}