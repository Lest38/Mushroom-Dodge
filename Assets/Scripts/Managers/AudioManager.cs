using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Music")]
    [SerializeField] private AudioClip menuMusic;
    [SerializeField] private AudioClip gameMusic;

    [Header("SFX")]
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private AudioClip newRecordSound;
    [SerializeField] private AudioClip fireballWhoosh;
    [SerializeField] private AudioClip shieldSound;
    [SerializeField] private AudioClip buttonClickSound;

    private bool isMusicEnabled = true;
    private bool isSFXEnabled = true;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadSettings();

        if (musicSource == null) musicSource = gameObject.AddComponent<AudioSource>();
        if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();

        musicSource.loop = true;
        musicSource.playOnAwake = false;
        sfxSource.playOnAwake = false;

        ApplyVolume();
        PlayMenuMusic();
    }

    void ApplyVolume()
    {
        musicSource.volume = isMusicEnabled ? 1f : 0f;
        sfxSource.volume = isSFXEnabled ? 0.7f : 0f;
    }

    public void PlayMenuMusic() => PlayMusic(menuMusic);
    public void PlayGameMusic() => PlayMusic(gameMusic);

    void PlayMusic(AudioClip clip)
    {
        if (!isMusicEnabled || clip == null) return;

        if (musicSource.isPlaying && musicSource.clip == clip) return;

        musicSource.clip = clip;
        musicSource.Play();
    }

    public void PlaySFX(string clipName)
    {
        if (!isSFXEnabled) return;

        AudioClip clip = clipName switch
        {
            "Death" => deathSound,
            "NewRecord" => newRecordSound,
            "Fireball" => fireballWhoosh,
            "Shield" => shieldSound,
            "ButtonClick" => buttonClickSound,
            _ => null
        };

        if (clip != null) sfxSource.PlayOneShot(clip);
    }

    public void ToggleMusic()
    {
        isMusicEnabled = !isMusicEnabled;
        musicSource.volume = isMusicEnabled ? 1f : 0f;

        if (isMusicEnabled && !musicSource.isPlaying)
            PlayMusic(GameManager.Instance?.IsGameRunning == true ? gameMusic : menuMusic);
        else if (!isMusicEnabled)
            musicSource.Stop();

        SaveSettings();
        PlaySFX("ButtonClick");
    }

    public bool IsMusicEnabled() => isMusicEnabled;
    public bool IsSFXEnabled() => isSFXEnabled;

    void SaveSettings()
    {
        PlayerPrefs.SetInt("MusicEnabled", isMusicEnabled ? 1 : 0);
        PlayerPrefs.SetInt("SFXEnabled", isSFXEnabled ? 1 : 0);
    }

    void LoadSettings()
    {
        isMusicEnabled = PlayerPrefs.GetInt("MusicEnabled", 1) == 1;
        isSFXEnabled = PlayerPrefs.GetInt("SFXEnabled", 1) == 1;
    }
}