using UnityEngine;

/// <summary>
/// Attached to every fish GameObject.
/// Handles movement, orientation, hit detection, damage and death.
/// VERSION 2.0 - Clean rewrite with correct orientation for all directions
/// </summary>
public class FishController : MonoBehaviour
{
    public FishData Data      { get; private set; }
    public int  CurrentHP     { get; private set; }
    public bool IsDead        { get; private set; } = false;

    private float _angle;
    private float _speed;
    private float _wAmp;
    private float _wFreq;
    private float _wPhase;
    private float _spriteOffset;
    private bool  _facesRight;

    private SpriteRenderer _sr;
    private Transform      _shadow;

    // Unique sort order per fish to prevent Z-fighting
    private static int _sortCounter = 0;

    // ─── Initialisation ────────────────────────────────────────────────────────

    public void Initialise(FishData data)
    {
        Data      = data;
        CurrentHP = data.hitPoints;

        switch (data.fishType)
        {
            case "turtle":
            case "hammerhead":
            case "clownfish":
            case "shark":
            case "arowana":
            case "squid":
            case "stingray":
            case "crab":
            case "goldfish":
            case "shrimp":
                _spriteOffset = -90f;
                _facesRight   = false;
                break;

            case "puffer":
                _spriteOffset = 90f;
                _facesRight   = false;
                break;

            case "mantaray":
                _spriteOffset = -90f;
                _facesRight   = false;
                break;

            case "lionfish":
            case "jellyfish":
            case "dragon":
            default:
                _spriteOffset = 0f;
                _facesRight   = true;
                break;
        }

        _sr = GetComponent<SpriteRenderer>();
        if (_sr != null && data.sprite != null)
        {
            _sr.sprite       = data.sprite;
            _sr.sortingOrder = _sortCounter++;
            if (_sortCounter > 10000) _sortCounter = 0;
        }

        FishAnimator anim = GetComponent<FishAnimator>();
        if (anim != null)
        {
            int maxFrames = 0;
            if (data.fishType == "clownfish") maxFrames = 31;
            anim.Initialise(data.sprite, 8f, maxFrames);
        }

        float diameter = data.WorldRadius * 2f;
        transform.localScale = new Vector3(diameter, diameter, 1f);

        Camera cam    = Camera.main;
        float screenL = cam.ViewportToWorldPoint(new Vector3(0, 0, 0)).x;
        float screenR = cam.ViewportToWorldPoint(new Vector3(1, 0, 0)).x;
        float screenB = cam.ViewportToWorldPoint(new Vector3(0, 0, 0)).y;
        float screenT = cam.ViewportToWorldPoint(new Vector3(0, 1, 0)).y;
        float w       = screenR - screenL;
        float h       = screenT - screenB;

        float left   = screenL + w * 0.06f;
        float right  = screenL + w * 0.94f;
        float bottom = screenB + h * 0.16f;
        float top    = screenB + h * 0.84f;

        _speed  = data.WorldSpeed * (0.8f + Random.value * 0.4f);
        _wAmp   = 0.15f + Random.value * 0.25f;
        _wFreq  = 0.5f  + Random.value;
        _wPhase = Random.value * Mathf.PI * 2f;

        float margin = data.WorldRadius;
        int   edge   = Random.Range(0, 4);
        float x, y;

        switch (edge)
        {
            case 0:
                x      = left   - margin;
                y      = Random.Range(bottom, top);
                _angle = (Random.value - 0.5f) * 0.8f;
                break;
            case 1:
                x      = right  + margin;
                y      = Random.Range(bottom, top);
                _angle = Mathf.PI + (Random.value - 0.5f) * 0.8f;
                break;
            case 2:
                x      = Random.Range(left, right);
                y      = top    + margin;
                _angle = -Mathf.PI / 2f + (Random.value - 0.5f) * 0.8f;
                break;
            default:
                x      = Random.Range(left, right);
                y      = bottom - margin;
                _angle = Mathf.PI / 2f + (Random.value - 0.5f) * 0.8f;
                break;
        }

        transform.position = new Vector3(x, y, 0f);
        ApplyOrientation(0f);

        _shadow = transform.Find("Shadow");
        if (_shadow != null)
        {
            bool showShadow = data.fishType != "crab" && data.fishType != "shrimp";
            _shadow.gameObject.SetActive(showShadow);
            if (showShadow)
            {
                float compensated     = 1.5f / diameter;
                _shadow.localScale    = new Vector3(compensated, compensated * 0.3f, 1f);
                _shadow.localPosition = new Vector3(0f, -0.15f, 0.1f);
                SpriteRenderer shadowSR = _shadow.GetComponent<SpriteRenderer>();
                if (shadowSR != null && _sr != null)
                    shadowSR.sortingOrder = _sr.sortingOrder - 1;
            }
        }

        GameState.Instance.ActiveFish.Add(this);
    }

    // ─── Update ────────────────────────────────────────────────────────────────

    private void Update()
    {
        if (IsDead) return;
        if (GameState.Instance.CurrentPhase != GameState.Phase.Playing) return;
        Move();
        CheckOffScreen();
        if (_shadow != null && _shadow.gameObject.activeSelf)
            _shadow.rotation = Quaternion.Euler(0f, 0f, _angle * Mathf.Rad2Deg);
    }

    private void Move()
    {
        float wob = Mathf.Sin(Time.time * _wFreq + _wPhase) * _wAmp;
        float ma  = _angle + wob;
        Vector3 pos = transform.position;
        pos.x += Mathf.Cos(ma) * _speed * Time.deltaTime;
        pos.y += Mathf.Sin(ma) * _speed * Time.deltaTime;
        transform.position = pos;
        ApplyOrientation(wob);
    }

    private void ApplyOrientation(float wob)
    {
        float travelDeg = (_angle + wob) * Mathf.Rad2Deg;
        bool  goingLeft = Mathf.Cos(_angle) < -0.5f;

        if (_sr != null) { _sr.flipX = false; _sr.flipY = false; }

        if (_facesRight)
        {
            if (goingLeft)
            {
                transform.rotation = Quaternion.Euler(0f, 0f, -(travelDeg - 180f) + _spriteOffset);
                if (_sr != null) _sr.flipX = true;
            }
            else
            {
                transform.rotation = Quaternion.Euler(0f, 0f, travelDeg + _spriteOffset);
            }
        }
        else
        {
            if (goingLeft)
            {
                transform.rotation = Quaternion.Euler(0f, 0f, 180f + travelDeg + _spriteOffset);
                if (_sr != null) _sr.flipY = true;
            }
            else
            {
                transform.rotation = Quaternion.Euler(0f, 0f, travelDeg + _spriteOffset);
            }
        }
    }

    private void CheckOffScreen()
    {
        Vector3 pos    = transform.position;
        float   margin = Data != null ? Data.WorldRadius + 0.5f : 1f;
        Camera  cam    = Camera.main;
        float   sLeft  = cam.ViewportToWorldPoint(Vector3.zero).x  - margin;
        float   sRight = cam.ViewportToWorldPoint(Vector3.right).x + margin;
        float   sBot   = cam.ViewportToWorldPoint(Vector3.zero).y  - margin;
        float   sTop   = cam.ViewportToWorldPoint(Vector3.up).y    + margin;
        if (pos.x < sLeft || pos.x > sRight || pos.y < sBot || pos.y > sTop)
            RemoveFromGame();
    }

    // ─── Damage & Death ────────────────────────────────────────────────────────

    public bool TakeHit(StationData shooter)
    {
        if (IsDead) return false;
        CurrentHP--;
        if (_sr != null) StartCoroutine(FlashWhite());
        if (CurrentHP <= 0) { Die(shooter); return true; }
        return false;
    }

    private void Die(StationData shooter)
    {
        IsDead = true;
        if (shooter != null)
        {
            shooter.Score += Data.pointValue;
            switch (Data.fishType)
            {
                case "clownfish": shooter.ClownfishKills++; break;
                case "puffer":    shooter.PufferKills++;    break;
                case "shark":     shooter.SharkKills++;     break;
            }
        }

        // Trigger death effect while fish is still alive
        if (FishDeathEffect.Instance != null)
            FishDeathEffect.Instance.SpawnBurstAt(transform.position, _sr, Data.WorldRadius);

        OnFishDied?.Invoke(this, shooter);
        RemoveFromGame();
    }

    public static event System.Action<FishController, StationData> OnFishDied;

    private void RemoveFromGame()
    {
        if (GameState.Instance != null)
            GameState.Instance.ActiveFish.Remove(this);
        Destroy(gameObject);
    }

    private System.Collections.IEnumerator FlashWhite()
    {
        if (_sr == null) yield break;
        Color original = _sr.color;
        _sr.color = Color.white;
        yield return new WaitForSeconds(0.05f);
        if (_sr != null) _sr.color = original;
    }

    public void SetWobble(float amp, float freq)
    {
        _wAmp  = amp;
        _wFreq = freq;
    }

    public bool ContainsPoint(Vector2 point)
    {
        return Vector2.Distance(transform.position, point) <= Data.WorldRadius;
    }
}
