using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip menuMusic;
    [SerializeField] private AudioClip gameMusic;
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private AudioClip newRecordSound;
    [SerializeField] private AudioClip fireballWhoosh;
    [SerializeField] private AudioClip shieldSound;
    [SerializeField] private AudioClip buttonClickSound;

    [Header("Settings")]
    [SerializeField] private float musicVolume = 0.5f;
    [SerializeField] private float sfxVolume = 0.7f;

    private bool isMusicEnabled = true;
    private bool isSFXEnabled = true;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadAudioSettings();
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitializeAudioSources();

        PlayMenuMusic();
    }

    void InitializeAudioSources()
    {
        if (musicSource != null)
        {
            musicSource.volume = isMusicEnabled ? musicVolume : 0f;
            musicSource.loop = true;
        }

        if (sfxSource != null)
        {
            sfxSource.volume = isSFXEnabled ? sfxVolume : 0f;
        }
    }

    public void PlayMenuMusic()
    {
        if (musicSource == null || menuMusic == null) return;

        if (!isMusicEnabled) return;

        if (musicSource.isPlaying && musicSource.clip == menuMusic) return;

        musicSource.clip = menuMusic;
        musicSource.Play();
    }

    public void PlayGameMusic()
    {
        if (musicSource == null || gameMusic == null) return;

        if (!isMusicEnabled) return;

        if (musicSource.isPlaying && musicSource.clip == gameMusic) return;

        musicSource.clip = gameMusic;
        musicSource.Play();
    }

    public void PlaySFX(string clipName)
    {
        if (sfxSource == null || !isSFXEnabled) return;

        AudioClip clip = null;

        switch (clipName)
        {
            case "Death": clip = deathSound; break;
            case "NewRecord": clip = newRecordSound; break;
            case "Fireball": clip = fireballWhoosh; break;
            case "Shield": clip = shieldSound; break;
            case "ButtonClick": clip = buttonClickSound; break;
        }

        if (clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    public void ToggleMusic()
    {
        isMusicEnabled = !isMusicEnabled;

        if (musicSource != null)
        {
            musicSource.volume = isMusicEnabled ? musicVolume : 0f;

            if (isMusicEnabled && !musicSource.isPlaying)
            {
                if (GameManager.Instance != null && GameManager.Instance.IsGameRunning)
                    PlayGameMusic();
                else
                    PlayMenuMusic();
            }
            else if (!isMusicEnabled)
            {
                musicSource.Stop();
            }
        }

        SaveAudioSettings();
        Debug.Log($"Музыка {(isMusicEnabled ? "включена" : "выключена")}");
    }

    public void ToggleSFX()
    {
        isSFXEnabled = !isSFXEnabled;

        if (sfxSource != null)
        {
            sfxSource.volume = isSFXEnabled ? sfxVolume : 0f;
        }

        SaveAudioSettings();
        Debug.Log($"Звуки эффектов {(isSFXEnabled ? "включены" : "выключены")}");
    }

    public bool IsMusicEnabled()
    {
        return isMusicEnabled;
    }

    public bool IsSFXEnabled()
    {
        return isSFXEnabled;
    }

    void SaveAudioSettings()
    {
        PlayerPrefs.SetInt("MusicEnabled", isMusicEnabled ? 1 : 0);
        PlayerPrefs.SetInt("SFXEnabled", isSFXEnabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    void LoadAudioSettings()
    {
        isMusicEnabled = PlayerPrefs.GetInt("MusicEnabled", 1) == 1;
        isSFXEnabled = PlayerPrefs.GetInt("SFXEnabled", 1) == 1;
    }

    public void DebugAudioState()
    {
        Debug.Log($"Музыка: {(isMusicEnabled ? "ON" : "OFF")}, SFX: {(isSFXEnabled ? "ON" : "OFF")}");
    }
}