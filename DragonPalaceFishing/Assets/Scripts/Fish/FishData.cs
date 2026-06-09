using UnityEngine;

/// <summary>
/// ScriptableObject that defines a single fish type.
/// Create one asset via: Assets → Create → Dragon Palace → Fish Data
/// </summary>
[CreateAssetMenu(fileName = "FishData", menuName = "Dragon Palace/Fish Data")]
public class FishData : ScriptableObject
{
    [Header("Identity")]
    public string fishType;
    public string displayLabel;
    public Sprite sprite;

    [Header("Stats")]
    public int   pointValue;
    public int   hitPoints;
    public float radius;
    public float speed;
    public int   spawnWeight;

    [Header("Sprite Orientation")]
    [Tooltip("Rotation offset in degrees to correct sprite sheet orientation. " +
             "Use 90 for sprites facing up, -90 for sprites facing down.")]
    public float spriteRotationOffset = 0f;

    [Header("Optional VFX")]
    public RuntimeAnimatorController animatorController;
    public Color labelColor = Color.white;

    // ─── Convenience ──────────────────────────────────────────────────────────
    public float WorldSpeed  => speed  / 100f;
    public float WorldRadius => radius / 100f;
}