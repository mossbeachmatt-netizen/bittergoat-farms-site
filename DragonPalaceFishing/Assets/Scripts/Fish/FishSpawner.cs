using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawns fish at random intervals using weighted probability.
/// Attach to the _GameState GameObject (or a dedicated _FishSpawner object).
/// </summary>
public class FishSpawner : MonoBehaviour
{
    // ─── Inspector Fields ──────────────────────────────────────────────────────

    [Header("Fish Roster")]
    [Tooltip("Drag all 15 FishData assets here in the Inspector.")]
    public List<FishData> FishRoster = new List<FishData>();

    [Header("Fish Prefab")]
    [Tooltip("A simple prefab with SpriteRenderer + FishController attached.")]
    public GameObject FishPrefab;

    [Header("Spawn Timing")]
    [Tooltip("Minimum seconds between spawns.")]
    public float MinSpawnInterval = 0.8f;

    [Tooltip("Maximum seconds between spawns.")]
    public float MaxSpawnInterval = 2.2f;

    [Tooltip("Maximum number of fish alive at once.")]
    public int MaxFishOnScreen = 20;

    // ─── Private State ─────────────────────────────────────────────────────────
    private float _nextSpawnTime = 0f;

    // ─── Unity Lifecycle ───────────────────────────────────────────────────────

    private void Update()
    {
        if (GameState.Instance.CurrentPhase != GameState.Phase.Playing) return;

        if (Time.time >= _nextSpawnTime)
        {
            TrySpawn();
            ScheduleNextSpawn();
        }
    }

    // ─── Spawning ──────────────────────────────────────────────────────────────

    private void TrySpawn()
    {
        // Don't exceed the screen cap
        if (GameState.Instance.ActiveFish.Count >= MaxFishOnScreen) return;

        FishData data = WeightedPick();
        if (data == null) return;

        SpawnFish(data);
    }

    private void SpawnFish(FishData data)
    {
        if (FishPrefab == null)
        {
            Debug.LogWarning("[FishSpawner] FishPrefab is not assigned!");
            return;
        }

        // Instantiate at origin — FishController.Initialise() sets the real position
        GameObject go = Instantiate(FishPrefab, Vector3.zero, Quaternion.identity);
        go.name = $"Fish_{data.displayLabel}";

        FishController fc = go.GetComponent<FishController>();
        if (fc == null)
        {
            Debug.LogError("[FishSpawner] FishPrefab is missing a FishController component!");
            Destroy(go);
            return;
        }

        fc.Initialise(data);
    }

    private void ScheduleNextSpawn()
    {
        _nextSpawnTime = Time.time + Random.Range(MinSpawnInterval, MaxSpawnInterval);
    }

    // ─── Weighted Random Pick ──────────────────────────────────────────────────

    /// <summary>
    /// Picks a FishData at random, weighted by each fish's spawnWeight.
    /// Higher weight = appears more often (matches HTML weightedPick logic).
    /// </summary>
    private FishData WeightedPick()
    {
        if (FishRoster == null || FishRoster.Count == 0)
        {
            Debug.LogWarning("[FishSpawner] FishRoster is empty!");
            return null;
        }

        // Sum all weights
        int totalWeight = 0;
        foreach (var fish in FishRoster)
            totalWeight += fish.spawnWeight;

        if (totalWeight <= 0) return null;

        // Roll a random number, then walk down the list
        int roll = Random.Range(0, totalWeight);
        int cumulative = 0;

        foreach (var fish in FishRoster)
        {
            cumulative += fish.spawnWeight;
            if (roll < cumulative)
                return fish;
        }

        // Fallback (should never reach here)
        return FishRoster[FishRoster.Count - 1];
    }

    // ─── Public Control ────────────────────────────────────────────────────────

    /// <summary>Call this when a new round starts to reset the spawn timer.</summary>
    public void ResetSpawner()
    {
        ScheduleNextSpawn();
    }
}