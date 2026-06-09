using UnityEngine;

/// <summary>
/// Manages all game audio — background music and sound effects.
/// Music starts when gameplay begins, stops on round end.
/// Singleton — attach to a dedicated _AudioManager GameObject.
/// </summary>
public class AudioManager : MonoBehaviour
{
    // ─── Singleton ─────────────────────────────────────────────────────────────
    public static AudioManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ─── Inspector Fields ──────────────────────────────────────────────────────

    [Header("Music")]
    public AudioClip BackgroundMusic;

    [Range(0f, 1f)]
    public float MusicVolume = 0.7f;

    [Header("Sound Effects")]
    public AudioClip SFXBulletFire;
    public AudioClip SFXLaserFire;
    public AudioClip SFXFishDie;
    public AudioClip SFXUpgrade;
    public AudioClip SFXDragonWarning;
    public AudioClip SFXDragonDie;
    public AudioClip SFXRoundEnd;

    [Range(0f, 1f)]
    public float SFXVolume = 1f;

    // ─── Audio Sources ─────────────────────────────────────────────────────────
    private AudioSource _musicSource;
    private AudioSource _sfxSource;

    // ─── Unity Lifecycle ───────────────────────────────────────────────────────

    private void Start()
    {
        _musicSource             = gameObject.AddComponent<AudioSource>();
        _musicSource.loop        = true;
        _musicSource.volume      = MusicVolume;
        _musicSource.playOnAwake = false;

        _sfxSource             = gameObject.AddComponent<AudioSource>();
        _sfxSource.loop        = false;
        _sfxSource.volume      = SFXVolume;
        _sfxSource.playOnAwake = false;

        GameState.Instance.OnPhaseChanged += OnPhaseChanged;
        FishController.OnFishDied         += OnFishDied;

        // Do NOT auto-play music on start
    }

    private void OnDestroy()
    {
        if (GameState.Instance != null)
            GameState.Instance.OnPhaseChanged -= OnPhaseChanged;
        FishController.OnFishDied -= OnFishDied;
    }

    // ─── Music Control ─────────────────────────────────────────────────────────

    public void PlayMusic()
    {
        if (_musicSource == null || BackgroundMusic == null) return;
        if (_musicSource.isPlaying) return;
        _musicSource.clip = BackgroundMusic;
        _musicSource.Play();
    }

    public void StopMusic()
    {
        if (_musicSource == null) return;
        _musicSource.Stop();
    }

    public void FadeOutMusic(float duration = 1f)
    {
        StartCoroutine(FadeMusicOut(duration));
    }

    private System.Collections.IEnumerator FadeMusicOut(float duration)
    {
        float startVol = _musicSource.volume;
        float elapsed  = 0f;

        while (elapsed < duration)
        {
            elapsed             += Time.deltaTime;
            _musicSource.volume  = Mathf.Lerp(startVol, 0f, elapsed / duration);
            yield return null;
        }

        _musicSource.Stop();
        _musicSource.volume = startVol;
    }

    public void SetMusicVolume(float vol)
    {
        MusicVolume = vol;
        if (_musicSource != null)
            _musicSource.volume = vol;
    }

    // ─── SFX ───────────────────────────────────────────────────────────────────

    public void PlaySFX(AudioClip clip)
    {
        if (_sfxSource == null || clip == null) return;
        _sfxSource.PlayOneShot(clip, SFXVolume);
    }

    public void PlayBulletFire(bool isLaser = false)
        => PlaySFX(isLaser ? SFXLaserFire : SFXBulletFire);

    public void PlayUpgrade()    => PlaySFX(SFXUpgrade);
    public void PlayDragonWarn() => PlaySFX(SFXDragonWarning);
    public void PlayDragonDie()  => PlaySFX(SFXDragonDie);
    public void PlayRoundEnd()   => PlaySFX(SFXRoundEnd);

    // ─── Phase Events ──────────────────────────────────────────────────────────

    private void OnPhaseChanged(GameState.Phase phase)
    {
        switch (phase)
        {
            case GameState.Phase.Playing:
                // Music starts exactly when gameplay begins
                PlayMusic();
                break;

            case GameState.Phase.RoundEnd:
                // Fade out music and play round end sound
                FadeOutMusic(1.5f);
                PlaySFX(SFXRoundEnd);
                break;

            case GameState.Phase.Countdown:
            case GameState.Phase.Options:
            case GameState.Phase.Title:
            case GameState.Phase.Lobby:
                // Silence during menus and countdown
                StopMusic();
                break;
        }
    }

    private void OnFishDied(FishController fish, StationData shooter)
    {
        if (fish == null || fish.Data == null) return;

        if (fish.Data.fishType == "dragon")
            PlaySFX(SFXDragonDie);
        else
            PlaySFX(SFXFishDie);
    }
}