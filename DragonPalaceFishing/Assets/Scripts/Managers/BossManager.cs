using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Manages the Dragon boss event.
/// - Watches the round timer
/// - Shows a WARNING message at 30 seconds remaining
/// - Spawns the Dragon fish after the warning
/// </summary>
public class BossManager : MonoBehaviour
{
    // ─── Inspector Fields ──────────────────────────────────────────────────────

    [Header("Timing")]
    public float BossTriggerTime  = 30f;
    public float WarningDuration  = 3.2f;

    [Header("Boss Stats")]
    public int   BossHP           = 55;
    public int   BossPoints       = 25000;
    public float BossSize         = 155f;
    public float BossSpeed        = 28f;

    [Header("Boss Fish Data")]
    public FishData DragonFishData;

    [Header("Fish Prefab")]
    public GameObject FishPrefab;

    [Header("Warning UI")]
    public TextMeshProUGUI WarningLabel;

    // ─── Runtime State ─────────────────────────────────────────────────────────
    private bool _warningShown = false;
    private bool _bossSpawned  = false;

    // ─── Unity Lifecycle ───────────────────────────────────────────────────────

    private void Start()
    {
        GameState.Instance.OnPhaseChanged += OnPhaseChanged;

        if (WarningLabel != null)
            WarningLabel.gameObject.SetActive(false);

        // Verify assignments
        Debug.Log($"[BossManager] DragonFishData={DragonFishData != null} " +
                  $"FishPrefab={FishPrefab != null} " +
                  $"WarningLabel={WarningLabel != null}");
    }

    private void OnDestroy()
    {
        if (GameState.Instance != null)
            GameState.Instance.OnPhaseChanged -= OnPhaseChanged;
    }

    private void Update()
    {
        if (GameState.Instance.CurrentPhase != GameState.Phase.Playing) return;
        if (_bossSpawned || _warningShown) return;

        if (GameState.Instance.TimeLeft <= BossTriggerTime)
        {
            Debug.Log($"[BossManager] Boss trigger! TimeLeft={GameState.Instance.TimeLeft}");
            _warningShown = true;
            StartCoroutine(BossSequence());
        }
    }

    // ─── Boss Sequence ─────────────────────────────────────────────────────────

    private IEnumerator BossSequence()
    {
        Debug.Log("[BossManager] Starting boss sequence...");
        ShowWarning(true);

        float elapsed = 0f;
        while (elapsed < WarningDuration)
        {
            elapsed += Time.deltaTime;
            if (WarningLabel != null)
            {
                float flash = Mathf.Sin(elapsed * 10f);
                WarningLabel.alpha = flash > 0 ? 1f : 0.3f;
            }
            yield return null;
        }

        ShowWarning(false);
        Debug.Log("[BossManager] Warning done — spawning dragon...");
        SpawnDragon();
    }

    // ─── Warning Display ───────────────────────────────────────────────────────

    private void ShowWarning(bool show)
    {
        if (WarningLabel == null) return;
        WarningLabel.gameObject.SetActive(show);
        if (show)
        {
            WarningLabel.text     = "WARNING!\nDRAGON APPROACHES!";
            WarningLabel.color    = new Color(1f, 0.2f, 0.2f, 1f);
            WarningLabel.fontSize = 60;
        }
    }

    // ─── Boss Spawn ────────────────────────────────────────────────────────────

    private void SpawnDragon()
    {
        _bossSpawned = true;

        if (FishPrefab == null)
        {
            Debug.LogError("[BossManager] FishPrefab is NULL!");
            return;
        }

        if (DragonFishData == null)
        {
            Debug.LogError("[BossManager] DragonFishData is NULL!");
            return;
        }

        Debug.Log("[BossManager] Instantiating dragon...");

        // Override dragon stats with boss values
        DragonFishData.hitPoints = BossHP;
        DragonFishData.radius    = BossSize;
        DragonFishData.speed     = BossSpeed;

        GameObject go = Instantiate(FishPrefab, Vector3.zero, Quaternion.identity);
        go.name = "DRAGON_BOSS";

        FishController fc = go.GetComponent<FishController>();
        if (fc == null)
        {
            Debug.LogError("[BossManager] FishPrefab is missing FishController component!");
            Destroy(go);
            return;
        }

        fc.Initialise(DragonFishData);

        // Flatten the wobble for the dragon — it is large and slow
        // so the default sine wave looks like sliding
        fc.SetWobble(amp: 0.03f, freq: 0.3f);

        Debug.Log($"[BossManager] Dragon spawned at {go.transform.position}");

        // Subscribe to detect boss death
        FishController.OnFishDied += OnBossDied;

        // Show arrival effect
        StartCoroutine(BossArrivalEffect());
    }

    private IEnumerator BossArrivalEffect()
    {
        if (WarningLabel == null) yield break;

        WarningLabel.gameObject.SetActive(true);
        WarningLabel.text     = "DRAGON!";
        WarningLabel.color    = new Color(1f, 0.6f, 0f, 1f);
        WarningLabel.fontSize = 80;
        WarningLabel.alpha    = 1f;

        float elapsed = 0f;
        while (elapsed < 1.5f)
        {
            elapsed += Time.deltaTime;
            if (WarningLabel != null)
                WarningLabel.alpha = 1f - (elapsed / 1.5f);
            yield return null;
        }

        if (WarningLabel != null)
            WarningLabel.gameObject.SetActive(false);
    }

    // ─── Boss Death ────────────────────────────────────────────────────────────

    private void OnBossDied(FishController fish, StationData shooter)
    {
        if (fish == null || fish.Data == null) return;
        if (fish.Data.fishType != "dragon") return;

        if (shooter != null)
            shooter.Score += BossPoints - fish.Data.pointValue;

        FishController.OnFishDied -= OnBossDied;
        Debug.Log($"[BossManager] Dragon slain by {shooter?.Label}!");

        StartCoroutine(BossDeathEffect());
    }

    private IEnumerator BossDeathEffect()
    {
        if (WarningLabel == null) yield break;

        WarningLabel.gameObject.SetActive(true);
        WarningLabel.text     = "DRAGON SLAIN!";
        WarningLabel.color    = new Color(1f, 0.84f, 0f, 1f);
        WarningLabel.fontSize = 70;
        WarningLabel.alpha    = 1f;

        float elapsed = 0f;
        while (elapsed < 2f)
        {
            elapsed += Time.deltaTime;
            if (WarningLabel != null)
                WarningLabel.alpha = 1f - (elapsed / 2f);
            yield return null;
        }

        if (WarningLabel != null)
            WarningLabel.gameObject.SetActive(false);
    }

    // ─── Phase Changes ─────────────────────────────────────────────────────────

    private void OnPhaseChanged(GameState.Phase phase)
    {
        if (phase == GameState.Phase.Countdown)
        {
            _warningShown  = false;
            _bossSpawned   = false;

            if (WarningLabel != null)
                WarningLabel.gameObject.SetActive(false);

            FishController.OnFishDied -= OnBossDied;
        }
    }
}