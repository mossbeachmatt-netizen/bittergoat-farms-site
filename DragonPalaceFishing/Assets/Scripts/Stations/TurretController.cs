using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles turret visuals for a single station.
/// - Rotates to aim at the player's mouse (player station) or nearest fish (AI)
/// - Swaps sprite based on active upgrade (normal/double/laser/triple)
/// - Shows glow ring matching station colour
/// </summary>
public class TurretController : MonoBehaviour
{
    // ─── Inspector Fields ──────────────────────────────────────────────────────

    [Header("Sprites")]
    public Sprite SpriteNormal;
    public Sprite SpriteDouble;
    public Sprite SpriteLaser;
    public Sprite SpriteTriple;

    [Header("Glow")]
    public SpriteRenderer GlowRenderer;

    // ─── Runtime ───────────────────────────────────────────────────────────────
    private StationData    _station;
    private SpriteRenderer _gunSR;
    private Transform      _gunTransform;
    private string         _lastUpgrade;

    // Cache the original Gun scale so we never override it
    private Vector3 _originalGunScale;

    // ─── Initialise ────────────────────────────────────────────────────────────

    public void Initialise(StationData station)
    {
        _station = station;

        Transform gun = transform.Find("Gun");
        if (gun != null)
        {
            _gunTransform     = gun;
            _gunSR            = gun.GetComponent<SpriteRenderer>();
            _originalGunScale = gun.localScale;   // remember the prefab scale
        }
        else
        {
            Debug.LogWarning("[TurretController] No child named 'Gun' found on " + gameObject.name);
        }

        transform.position = new Vector3(station.Position.x, station.Position.y, 0f);

        UpdateGlow();
        UpdateSprite();
    }

    // ─── Update ────────────────────────────────────────────────────────────────

    private void Update()
    {
        if (_station == null) return;
        if (GameState.Instance.CurrentPhase != GameState.Phase.Playing &&
            GameState.Instance.CurrentPhase != GameState.Phase.Countdown) return;

        AimGun();

        // Detect upgrade change and update visuals
        if (_station.ActiveUpgrade != _lastUpgrade)
        {
            UpdateSprite();
            UpdateGlow();
            _lastUpgrade = _station.ActiveUpgrade;
            Debug.Log($"[TurretController] {_station.Label} upgrade → {_station.ActiveUpgrade ?? "none"}");
        }
    }

    // ─── Aiming ────────────────────────────────────────────────────────────────

    private void AimGun()
    {
        if (_gunTransform == null) return;

        Vector2 targetPos;

        if (_station.IsPlayer)
        {
            Vector2 mouseScreen = Input.mousePosition;
            targetPos = Camera.main.ScreenToWorldPoint(mouseScreen);
        }
        else
        {
            FishController nearest = GetNearestFish();
            if (nearest == null) return;
            targetPos = nearest.transform.position;
        }

        Vector2 dir   = targetPos - (Vector2)transform.position;
        float   angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        _gunTransform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private FishController GetNearestFish()
    {
        var fish = GameState.Instance.ActiveFish;
        if (fish.Count == 0) return null;

        FishController nearest = null;
        float          minDist = float.MaxValue;

        foreach (var f in fish)
        {
            if (f == null || f.IsDead) continue;
            float dist = Vector2.Distance(transform.position, f.transform.position);
            if (dist < minDist) { minDist = dist; nearest = f; }
        }
        return nearest;
    }

    // ─── Sprite Swap ───────────────────────────────────────────────────────────

    private void UpdateSprite()
    {
        if (_gunSR == null) return;

        switch (_station?.ActiveUpgrade)
        {
            case "double": _gunSR.sprite = SpriteDouble ?? SpriteNormal; break;
            case "laser":  _gunSR.sprite = SpriteLaser  ?? SpriteNormal; break;
            case "triple": _gunSR.sprite = SpriteTriple ?? SpriteNormal; break;
            default:       _gunSR.sprite = SpriteNormal;                 break;
        }

        // Restore the original prefab scale — never override it
        if (_gunTransform != null)
            _gunTransform.localScale = _originalGunScale;
    }

    // ─── Glow ──────────────────────────────────────────────────────────────────

    private void UpdateGlow()
    {
        if (GlowRenderer == null) return;

        Color glowColor;

        switch (_station?.ActiveUpgrade)
        {
            case "double": glowColor = new Color(0.0f, 1.0f, 0.8f,  0.6f); break; // cyan
            case "laser":  glowColor = new Color(0.0f, 0.8f, 1.0f,  0.6f); break; // blue
            case "triple": glowColor = new Color(1.0f, 0.84f, 0.0f, 0.6f); break; // gold
            default:
                glowColor = _station != null
                    ? new Color(_station.Color.r, _station.Color.g, _station.Color.b, 0.35f)
                    : new Color(1f, 1f, 1f, 0.2f);
                break;
        }

        GlowRenderer.color = glowColor;
    }
}