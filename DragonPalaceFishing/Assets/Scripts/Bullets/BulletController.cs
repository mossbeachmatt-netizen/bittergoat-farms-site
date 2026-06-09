using UnityEngine;

/// <summary>
/// Attached to every bullet GameObject.
/// Travels in a straight line, tests for fish hits, then destroys itself.
/// Supports laser beam visual mode.
/// </summary>
public class BulletController : MonoBehaviour
{
    // ─── Configuration ─────────────────────────────────────────────────────────
    public float       Speed       { get; private set; } = 12f;
    public StationData Shooter     { get; private set; }
    public float       MaxLifetime { get; private set; } = 3f;
    public bool        IsLaser     { get; private set; } = false;

    [Header("Laser Sprite")]
    public Sprite LaserSprite;

    [Header("Bullet Scale")]
    [Tooltip("Scale of normal bullets")]
    public float NormalBulletScale = 0.6f;

    [Tooltip("Scale of laser beam")]
    public float LaserBulletScale = 0.3f;

    // ─── Private State ─────────────────────────────────────────────────────────
    private Vector2        _direction;
    private float          _age = 0f;
    private SpriteRenderer _sr;

    // ─── Initialise ────────────────────────────────────────────────────────────

    public void Initialise(Vector2 startPos, Vector2 targetPos,
                           StationData shooter, float speed = 12f,
                           bool isLaser = false)
    {
        transform.position = startPos;
        Shooter            = shooter;
        Speed              = speed;
        IsLaser            = isLaser;

        _direction = (targetPos - startPos).normalized;
        _sr        = GetComponent<SpriteRenderer>();

        // Rotate to face direction of travel
        float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        if (isLaser)
            ApplyLaserVisuals();
        else
            ApplyNormalVisuals(shooter);

        GameState.Instance.ActiveBullets.Add(this);
    }

    // ─── Visuals ───────────────────────────────────────────────────────────────

    private void ApplyNormalVisuals(StationData shooter)
    {
        if (_sr == null) return;

        // Colour bullet to match station colour
        if (shooter != null)
            _sr.color = new Color(shooter.Color.r, shooter.Color.g, shooter.Color.b, 1f);
        else
            _sr.color = Color.yellow;

        // Double the bullet size compared to before
        transform.localScale = new Vector3(NormalBulletScale, NormalBulletScale, 1f);
    }

    private void ApplyLaserVisuals()
    {
        if (_sr == null) return;

        // Load laser sprite from Resources
        if (LaserSprite == null)
            LaserSprite = Resources.Load<Sprite>("Sprites/bullet_laser");

        if (LaserSprite != null)
            _sr.sprite = LaserSprite;

        // Cyan color tint
        _sr.color = new Color(0f, 1f, 0.9f, 1f);

        // Shorter thinner beam
        transform.localScale = new Vector3(1.5f, LaserBulletScale, 1f);
    }

    // ─── Update ────────────────────────────────────────────────────────────────

    private void Update()
    {
        if (GameState.Instance.CurrentPhase != GameState.Phase.Playing)
        {
            RemoveFromGame();
            return;
        }

        _age += Time.deltaTime;
        if (_age >= MaxLifetime)
        {
            RemoveFromGame();
            return;
        }

        Move();
        CheckHits();
    }

    private void Move()
    {
        transform.position += (Vector3)(_direction * Speed * Time.deltaTime);
    }

    // ─── Hit Detection ─────────────────────────────────────────────────────────

    private void CheckHits()
    {
        Vector2 pos = transform.position;

        var fishList = new System.Collections.Generic.List<FishController>(
            GameState.Instance.ActiveFish);

        foreach (FishController fish in fishList)
        {
            if (fish == null || fish.IsDead) continue;

            float hitRadius = IsLaser
                ? fish.Data.WorldRadius * 1.2f
                : fish.Data.WorldRadius;

            if (Vector2.Distance(pos, fish.transform.position) <= hitRadius)
            {
                fish.TakeHit(Shooter);
                RemoveFromGame();
                return;
            }
        }
    }

    // ─── Cleanup ───────────────────────────────────────────────────────────────

    private void RemoveFromGame()
    {
        if (GameState.Instance != null)
            GameState.Instance.ActiveBullets.Remove(this);
        Destroy(gameObject);
    }

    private void OnBecameInvisible()
    {
        RemoveFromGame();
    }
}