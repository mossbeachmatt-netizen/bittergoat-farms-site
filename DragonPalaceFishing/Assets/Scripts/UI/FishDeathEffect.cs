using System.Collections;
using UnityEngine;

/// <summary>
/// Spawns a pixel burst effect when a fish dies.
/// Called directly from FishController.Die().
/// </summary>
public class FishDeathEffect : MonoBehaviour
{
    public static FishDeathEffect Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    [Header("Burst Settings")]
    public int   ParticleCount     = 40;
    public float BurstRadius       = 2.0f;
    public float BurstDuration     = 0.3f;
    public float ParticleSize      = 0.15f;

    private Sprite _squareSprite;

    private void Start()
    {
        Debug.Log("[FishDeathEffect] Ready");
    }

    // ─── Public entry point called from FishController ─────────────────────────

    public void SpawnBurstAt(Vector3 position, SpriteRenderer sr, float fishRadius)
    {
        Color color = new Color(1f, 0.85f, 0.1f); // gold default
        Debug.Log($"[FishDeathEffect] SpawnBurstAt {position} radius={fishRadius}");
        StartCoroutine(SpawnBurst(position, color, fishRadius));
    }

    // ─── Burst Coroutine ───────────────────────────────────────────────────────

    private IEnumerator SpawnBurst(Vector3 position, Color color, float fishRadius)
    {
        int   count    = Mathf.Clamp((int)(ParticleCount * (fishRadius / 0.5f)), 20, 60);
        float baseSize = Mathf.Max(0.15f, ParticleSize + fishRadius * 0.08f);

        GameObject[] particles  = new GameObject[count];
        Vector3[]    velocities = new Vector3[count];

        for (int i = 0; i < count; i++)
        {
            float angle   = Random.Range(0f, Mathf.PI * 2f);
            float speed   = Random.Range(0.5f, 1.0f);
            velocities[i] = new Vector3(
                Mathf.Cos(angle) * speed * BurstRadius,
                Mathf.Sin(angle) * speed * BurstRadius, 0f);

            float size = baseSize * Random.Range(0.5f, 1.5f);

            GameObject go       = new GameObject("BurstParticle");
            go.transform.position   = position;
            go.transform.localScale = new Vector3(size, size, 1f);
            go.transform.rotation   = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));

            SpriteRenderer psr      = go.AddComponent<SpriteRenderer>();
            psr.sprite              = GetSquareSprite();
            psr.color               = color;
            psr.sortingLayerName    = "Turrets";  // top sorting layer
            psr.sortingOrder        = 999;

            particles[i] = go;
        }

        float elapsed = 0f;
        while (elapsed < BurstDuration)
        {
            elapsed += Time.deltaTime;
            float t       = elapsed / BurstDuration;
            float easeOut = 1f - t * t;
            float alpha   = 1f - t;

            for (int i = 0; i < count; i++)
            {
                if (particles[i] == null) continue;
                // easeOut: starts fast, slows down — moves AWAY from center
                float outward = t * (2f - t); // ease out curve: 0 at t=0, 1 at t=1
                particles[i].transform.position   = position + velocities[i] * outward;
                float scale = 1f - t * 0.7f;
                particles[i].transform.localScale = new Vector3(baseSize * scale, baseSize * scale, 1f);
                SpriteRenderer psr = particles[i].GetComponent<SpriteRenderer>();
                if (psr != null) { Color c = color; c.a = alpha; psr.color = c; }
            }
            yield return null;
        }

        for (int i = 0; i < count; i++)
            if (particles[i] != null) Destroy(particles[i]);
    }

    // ─── Square Sprite ─────────────────────────────────────────────────────────

    private Sprite GetSquareSprite()
    {
        if (_squareSprite != null) return _squareSprite;
        Texture2D tex    = new Texture2D(4, 4, TextureFormat.RGBA32, false);
        Color[]   pixels = new Color[16];
        for (int i = 0; i < 16; i++) pixels[i] = Color.white;
        tex.SetPixels(pixels);
        tex.Apply();
        tex.filterMode = FilterMode.Point;
        _squareSprite   = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
        return _squareSprite;
    }
}