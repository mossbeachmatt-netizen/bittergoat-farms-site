using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Positions all 6 player slots, spawns turrets, fires bullets for the human player
/// and AI stations, and manages the upgrade system.
/// Bullets spawn from the barrel tip of the turret, not the center.
/// </summary>
public class StationManager : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject BulletPrefab;
    public GameObject TurretPrefab;

    [Header("Bullet")]
    public float BulletSpeed = 12f;

    [Header("Barrel Tip Offset")]
    [Tooltip("World units from turret center to barrel tip")]
    public float BarrelLength = 0.6f;

    [Header("Player Settings")]
    public int PlayerSlotIndex = 1;

    [Range(1, 5)]
    public int AICount = 5;

    [Header("Upgrade Thresholds")]
    public int ClownfishKillsForDouble = 3;
    public int PufferKillsForLaser     = 3;
    public int SharkKillsForTriple     = 2;

    [Header("Upgrade Duration (seconds)")]
    public float UpgradeDuration = 8f;

    // ─── Slot Definitions ──────────────────────────────────────────────────────

    private struct SlotDef
    {
        public int   Id;
        public int   Col;
        public int   Row;
        public Color Color;
    }

    private static readonly SlotDef[] SlotDefs = new SlotDef[]
    {
        new SlotDef { Id=0, Col=0, Row=1, Color=new Color(0.20f,1.00f,0.53f) },
        new SlotDef { Id=1, Col=1, Row=1, Color=new Color(1.00f,0.27f,0.40f) },
        new SlotDef { Id=2, Col=2, Row=1, Color=new Color(1.00f,0.87f,0.13f) },
        new SlotDef { Id=5, Col=0, Row=0, Color=new Color(1.00f,0.67f,0.20f) },
        new SlotDef { Id=6, Col=1, Row=0, Color=new Color(0.53f,1.00f,0.53f) },
        new SlotDef { Id=7, Col=2, Row=0, Color=new Color(0.80f,0.80f,1.00f) },
    };

    // ─── Private State ─────────────────────────────────────────────────────────

    private float[]                _aiFireTimers;
    private static readonly float[] AIRates = { 1.7f, 1.9f, 2.0f, 1.8f, 1.5f };
    private List<TurretController> _turrets = new List<TurretController>();
    private bool                   _mouseWasDown = false;

    // ─── Unity Lifecycle ───────────────────────────────────────────────────────

    private void Start()
    {
        // Don't build stations on startup — wait for Countdown phase
        // This prevents turrets showing on the title/splash screen
        GameState.Instance.OnPhaseChanged += OnPhaseChanged;
        FishController.OnFishDied         += OnFishDied;
    }

    private void OnDestroy()
    {
        if (GameState.Instance != null)
            GameState.Instance.OnPhaseChanged -= OnPhaseChanged;
        FishController.OnFishDied -= OnFishDied;
    }

    // ─── Update ────────────────────────────────────────────────────────────────

    private void Update()
    {
        CheckMouseInput();
        if (GameState.Instance.CurrentPhase != GameState.Phase.Playing) return;
        HandleAI();
        TickUpgrades();
    }

    // ─── Mouse Input ───────────────────────────────────────────────────────────

    private void CheckMouseInput()
    {
        bool clickDetected = false;

        if (Input.GetMouseButtonDown(0))
            clickDetected = true;

        if (!clickDetected && Mouse.current != null)
        {
            bool isDown = Mouse.current.leftButton.isPressed;
            if (isDown && !_mouseWasDown)
                clickDetected = true;
            _mouseWasDown = isDown;
        }

        if (clickDetected && GameState.Instance.CurrentPhase == GameState.Phase.Playing)
            HandlePlayerFire();
    }

    private void HandlePlayerFire()
    {
        StationData player = GameState.Instance.PlayerStation;
        if (player == null) return;

        Vector3 mouseScreen = Input.mousePosition;
        mouseScreen.z       = Mathf.Abs(Camera.main.transform.position.z);
        Vector2 clickWorld  = Camera.main.ScreenToWorldPoint(mouseScreen);

        FireBullet(player, clickWorld);

        if (player.ActiveUpgrade == "double")
            FireBulletAtAngleOffset(player, clickWorld, +5f);

        if (player.ActiveUpgrade == "triple")
        {
            FireBulletAtAngleOffset(player, clickWorld, +12f);
            FireBulletAtAngleOffset(player, clickWorld, -12f);
        }
    }

    // ─── Build Stations ────────────────────────────────────────────────────────

    private void BuildStations()
    {
        foreach (var t in _turrets)
            if (t != null) Destroy(t.gameObject);
        _turrets.Clear();

        GameState.Instance.Stations.Clear();

        Camera cam    = Camera.main;
        float  top    = cam.ViewportToWorldPoint(new Vector3(0, 1, 0)).y;
        float  bottom = cam.ViewportToWorldPoint(new Vector3(0, 0, 0)).y;
        float  left   = cam.ViewportToWorldPoint(new Vector3(0, 0, 0)).x;
        float  right  = cam.ViewportToWorldPoint(new Vector3(1, 0, 0)).x;

        float xMargin = (right - left)   * 0.12f;
        float yMargin = (top   - bottom) * 0.06f;

        float[] xs = { left  + xMargin, (left + right) * 0.5f, right - xMargin };
        float[] ys = { top   - yMargin, bottom + yMargin };

        SlotDef pDef   = SlotDefs[PlayerSlotIndex];
        var     aiPool = new List<SlotDef>();

        foreach (var sd in SlotDefs)
            if (sd.Id != pDef.Id) aiPool.Add(sd);

        var player = MakeStation(pDef, xs, ys, isPlayer: true, label: "YOU", aiRateIndex: -1);
        GameState.Instance.Stations.Add(player);
        SpawnTurret(player);

        int aiAdded = 0;
        foreach (var sd in aiPool)
        {
            if (aiAdded >= AICount) break;
            var ai = MakeStation(sd, xs, ys, isPlayer: false,
                                 label: $"P{aiAdded + 1}", aiRateIndex: aiAdded);
            GameState.Instance.Stations.Add(ai);
            SpawnTurret(ai);
            aiAdded++;
        }

        _aiFireTimers = new float[GameState.Instance.Stations.Count];
    }

    private StationData MakeStation(SlotDef sd, float[] xs, float[] ys,
                                    bool isPlayer, string label, int aiRateIndex)
    {
        float x     = xs[sd.Col];
        float y     = ys[sd.Row];
        float angle = (sd.Row == 0) ? -Mathf.PI / 2f : Mathf.PI / 2f;

        return new StationData
        {
            Id         = sd.Id,
            IsPlayer   = isPlayer,
            Label      = label,
            Color      = sd.Color,
            Score      = 0,
            Position   = new Vector2(x, y),
            Angle      = angle,
            AIFireRate = (aiRateIndex >= 0 && aiRateIndex < AIRates.Length)
                         ? AIRates[aiRateIndex] : 0f
        };
    }

    // ─── Turret Spawning ───────────────────────────────────────────────────────

    private void SpawnTurret(StationData station)
    {
        if (TurretPrefab == null)
        {
            Debug.LogWarning("[StationManager] TurretPrefab not assigned!");
            return;
        }

        GameObject       go = Instantiate(TurretPrefab);
        TurretController tc = go.GetComponent<TurretController>();

        if (tc == null)
        {
            Debug.LogError("[StationManager] TurretPrefab missing TurretController!");
            Destroy(go);
            return;
        }

        tc.Initialise(station);
        _turrets.Add(tc);
    }

    // ─── AI Logic ──────────────────────────────────────────────────────────────

    private void HandleAI()
    {
        var stations = GameState.Instance.Stations;
        var fish     = GameState.Instance.ActiveFish;

        for (int i = 0; i < stations.Count; i++)
        {
            StationData st = stations[i];
            if (st.IsPlayer || st.AIFireRate <= 0f) continue;

            _aiFireTimers[i] -= Time.deltaTime;
            if (_aiFireTimers[i] > 0f) continue;

            _aiFireTimers[i] = 1f / st.AIFireRate;

            if (fish.Count == 0) continue;

            FishController target = fish[Random.Range(0, fish.Count)];
            if (target == null) continue;

            FireBullet(st, target.transform.position);

            if (st.ActiveUpgrade == "double")
                FireBulletAtAngleOffset(st, target.transform.position, +5f);

            if (st.ActiveUpgrade == "triple")
            {
                FireBulletAtAngleOffset(st, target.transform.position, +12f);
                FireBulletAtAngleOffset(st, target.transform.position, -12f);
            }
        }
    }

    // ─── Bullet Firing ─────────────────────────────────────────────────────────

    private Vector2 GetBarrelTipPosition(StationData shooter, Vector2 targetWorld)
    {
        Vector2 dir = (targetWorld - shooter.Position).normalized;
        return shooter.Position + dir * BarrelLength;
    }

    private void FireBullet(StationData shooter, Vector2 targetWorld)
    {
        if (BulletPrefab == null)
        {
            Debug.LogWarning("[StationManager] BulletPrefab not assigned!");
            return;
        }

        bool    isLaser  = shooter.ActiveUpgrade == "laser";
        float   speed    = isLaser ? BulletSpeed * 2.5f : BulletSpeed;
        Vector2 spawnPos = GetBarrelTipPosition(shooter, targetWorld);

        GameObject       go = Instantiate(BulletPrefab);
        BulletController bc = go.GetComponent<BulletController>();

        if (bc == null)
        {
            Debug.LogError("[StationManager] BulletPrefab missing BulletController!");
            Destroy(go);
            return;
        }

        bc.Initialise(spawnPos, targetWorld, shooter, speed, isLaser);

        // Play fire sound — only for player station to avoid audio chaos from AI
        if (shooter.IsPlayer && AudioManager.Instance != null)
            AudioManager.Instance.PlayBulletFire(isLaser);
    }

    private void FireBulletAtAngleOffset(StationData shooter, Vector2 targetWorld, float offsetDeg)
    {
        Vector2 dir     = (targetWorld - shooter.Position).normalized;
        float   rad     = offsetDeg * Mathf.Deg2Rad;
        float   cos     = Mathf.Cos(rad);
        float   sin     = Mathf.Sin(rad);
        Vector2 rotated = new Vector2(
            dir.x * cos - dir.y * sin,
            dir.x * sin + dir.y * cos);

        Vector2 newTarget = shooter.Position + rotated * 10f;
        Vector2 spawnPos  = shooter.Position + rotated * BarrelLength;

        if (BulletPrefab == null) return;

        bool    isLaser = shooter.ActiveUpgrade == "laser";
        float   speed   = isLaser ? BulletSpeed * 2.5f : BulletSpeed;

        GameObject       go = Instantiate(BulletPrefab);
        BulletController bc = go.GetComponent<BulletController>();
        if (bc == null) { Destroy(go); return; }

        bc.Initialise(spawnPos, newTarget, shooter, speed, isLaser);
        // No sound for angle-offset bullets — primary FireBullet already played it
    }

    // ─── Upgrade System ────────────────────────────────────────────────────────

    private void OnFishDied(FishController fish, StationData shooter)
    {
        if (shooter == null) return;
        CheckUpgradeTriggers(shooter);
    }

    private void CheckUpgradeTriggers(StationData st)
    {
        if (st.ClownfishKills >= ClownfishKillsForDouble)
        {
            st.ClownfishKills = 0;
            ApplyUpgrade(st, "double");
        }
        else if (st.PufferKills >= PufferKillsForLaser)
        {
            st.PufferKills = 0;
            ApplyUpgrade(st, "laser");
        }
        else if (st.SharkKills >= SharkKillsForTriple)
        {
            st.SharkKills = 0;
            ApplyUpgrade(st, "triple");
        }
    }

    private void ApplyUpgrade(StationData st, string type)
    {
        st.ActiveUpgrade = type;
        st.UpgradeTimer  = UpgradeDuration;

        // Play upgrade sound
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayUpgrade();

        Debug.Log($"[StationManager] {st.Label} got upgrade: {type}");
    }

    private void TickUpgrades()
    {
        foreach (StationData st in GameState.Instance.Stations)
        {
            if (st.ActiveUpgrade == null) continue;
            st.UpgradeTimer -= Time.deltaTime;
            if (st.UpgradeTimer <= 0f)
            {
                st.ActiveUpgrade = null;
                st.UpgradeTimer  = 0f;
            }
        }
    }

    // ─── Phase Changes ─────────────────────────────────────────────────────────

    private void OnPhaseChanged(GameState.Phase phase)
    {
        if (phase == GameState.Phase.Countdown)
            BuildStations();
    }
}