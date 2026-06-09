using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Central singleton that holds all live game data.
/// Access from any script via:  GameState.Instance.someProperty
/// </summary>
public class GameState : MonoBehaviour
{
    // ─── Singleton ─────────────────────────────────────────────────────────────
    public static GameState Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ─── Game Phase ────────────────────────────────────────────────────────────
    public enum Phase
    {
        Title,
        Lobby,
        Matchmaking,
        Options,
        Countdown,
        Playing,
        RoundEnd
    }

    public Phase CurrentPhase { get; private set; } = Phase.Title;

    /// <summary>Call this to transition between game phases.</summary>
    public void SetPhase(Phase newPhase)
    {
        CurrentPhase = newPhase;
        OnPhaseChanged?.Invoke(newPhase);
        Debug.Log($"[GameState] Phase → {newPhase}");
    }

    // Other systems subscribe to this to react to phase changes
    public event System.Action<Phase> OnPhaseChanged;

    // ─── Round Timer ───────────────────────────────────────────────────────────
    public float RoundDuration = 60f;   // seconds per round
    public float TimeLeft      { get; private set; }
    public int   CountdownVal  { get; private set; } = 3;

    // ─── Fish & Bullets ────────────────────────────────────────────────────────
    // These lists are managed by FishSpawner and BulletController respectively.
    // GameState just holds the references so any script can find them.
    public List<FishController> ActiveFish    { get; } = new List<FishController>();
    public List<BulletController> ActiveBullets { get; } = new List<BulletController>();

    // ─── Stations / Scores ─────────────────────────────────────────────────────
    // Populated by StationManager on game start.
    public List<StationData> Stations { get; } = new List<StationData>();

    /// <summary>Returns the human player's station, or null if not found.</summary>
    public StationData PlayerStation =>
        Stations.Find(s => s.IsPlayer);

    // ─── Round Update Loop ─────────────────────────────────────────────────────
    private void Update()
    {
        if (CurrentPhase == Phase.Countdown)
        {
            TickCountdown();
        }
        else if (CurrentPhase == Phase.Playing)
        {
            TickPlaying();
        }
    }

    private float _countdownAccum = 0f;

    private void TickCountdown()
    {
        _countdownAccum += Time.deltaTime;
        if (_countdownAccum >= 1f)
        {
            _countdownAccum -= 1f;
            CountdownVal--;
            if (CountdownVal <= 0)
            {
                CountdownVal = 3;   // reset for next round
                TimeLeft = RoundDuration;
                SetPhase(Phase.Playing);
            }
        }
    }

    private void TickPlaying()
    {
        TimeLeft -= Time.deltaTime;
        if (TimeLeft <= 0f)
        {
            TimeLeft = 0f;
            SetPhase(Phase.RoundEnd);
        }
    }

    // ─── Round Control ─────────────────────────────────────────────────────────

    /// <summary>Call this from the UI/lobby when the player hits Play.</summary>
    public void StartRound()
    {
        // Clear leftover fish and bullets from a previous round
        foreach (var fish in ActiveFish)
            if (fish != null) Destroy(fish.gameObject);
        ActiveFish.Clear();

        foreach (var bullet in ActiveBullets)
            if (bullet != null) Destroy(bullet.gameObject);
        ActiveBullets.Clear();

        // Reset scores
        foreach (var station in Stations)
            station.Score = 0;

        CountdownVal  = 3;
        _countdownAccum = 0f;
        SetPhase(Phase.Countdown);
    }
}

// ─── StationData (simple data class, no MonoBehaviour needed) ──────────────────
/// <summary>
/// Holds runtime data for one player slot (human or AI).
/// Populated and owned by StationManager.
/// </summary>
[System.Serializable]
public class StationData
{
    public int     Id;
    public bool    IsPlayer;
    public string  Label;       // "YOU" or "P1"–"P5"
    public Color   Color;
    public int     Score;

    // Position (set by StationManager based on screen size)
    public Vector2 Position;
    public float   Angle;       // radians — direction the cannon faces

    // Upgrade state
    public string  ActiveUpgrade;   // null | "double" | "laser" | "triple"
    public float   UpgradeTimer;
    public bool    IsUpgraded   => ActiveUpgrade == "double";
    public bool    IsLaserMode  => ActiveUpgrade == "laser";
    public bool    IsTripleMode => ActiveUpgrade == "triple";

    // AI fire rate (shots per second; 0 = human player)
    public float   AIFireRate;

    // Kill counters that trigger upgrades
    public int     ClownfishKills;
    public int     PufferKills;
    public int     SharkKills;
}