using UnityEngine;

/// <summary>
/// Cycles through sprite sheet frames to animate a fish.
/// Attach to the FishPrefab alongside FishController.
/// Respects MaxFrames to avoid animating empty slots at end of sheet.
/// </summary>
public class FishAnimator : MonoBehaviour
{
    private Sprite[]       _frames;
    private SpriteRenderer _sr;
    private float          _fps         = 8f;
    private float          _timer       = 0f;
    private int            _frameIndex  = 0;
    private float          _phaseOffset = 0f;
    private int            _maxFrames   = 0;   // 0 = use all frames

    // ─── Initialise ────────────────────────────────────────────────────────────

    public void Initialise(Sprite baseSprite, float fps = 8f, int maxFrames = 0)
    {
        _sr          = GetComponent<SpriteRenderer>();
        _fps         = fps;
        _phaseOffset = Random.Range(0f, 1f);
        _maxFrames   = maxFrames;

        if (baseSprite == null)
        {
            Debug.LogWarning("[FishAnimator] No base sprite provided.");
            return;
        }

        _frames = LoadFramesFromSheet(baseSprite);

        if (_frames != null && _frames.Length > 0)
        {
            // Clamp to maxFrames if specified
            if (_maxFrames > 0 && _maxFrames < _frames.Length)
            {
                System.Array.Resize(ref _frames, _maxFrames);
                Debug.Log($"[FishAnimator] Clamped to {_maxFrames} frames for {baseSprite.texture.name}");
            }

            _frameIndex = Mathf.FloorToInt(_phaseOffset * _frames.Length);
            if (_sr != null)
                _sr.sprite = _frames[_frameIndex];
        }
    }

    // ─── Update ────────────────────────────────────────────────────────────────

    private void Update()
    {
        if (_frames == null || _frames.Length <= 1) return;
        if (GameState.Instance.CurrentPhase != GameState.Phase.Playing) return;

        _timer += Time.deltaTime;

        if (_timer >= 1f / _fps)
        {
            _timer      -= 1f / _fps;
            _frameIndex  = (_frameIndex + 1) % _frames.Length;

            if (_sr != null && _frames[_frameIndex] != null)
                _sr.sprite = _frames[_frameIndex];
        }
    }

    // ─── Frame Loading ─────────────────────────────────────────────────────────

    private Sprite[] LoadFramesFromSheet(Sprite baseSprite)
    {
        string textureName = baseSprite.texture.name;
        Sprite[] allSprites = Resources.LoadAll<Sprite>("Sprites/" + textureName);

        if (allSprites != null && allSprites.Length > 0)
        {
            System.Array.Sort(allSprites, (a, b) =>
                System.String.Compare(a.name, b.name, System.StringComparison.Ordinal));
            return allSprites;
        }

        Debug.LogWarning($"[FishAnimator] Could not load frames for {textureName}. " +
                          "Make sure sprites are in Assets/Resources/Sprites/");
        return new Sprite[] { baseSprite };
    }
}