using UnityEngine;

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

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayMenuMusic()
    {
        musicSource.clip = menuMusic;
        musicSource.Play();
    }

    public void PlayGameMusic()
    {
        musicSource.clip = gameMusic;
        musicSource.Play();
    }

    public void PlaySFX(string clipName)
    {
        AudioClip clip = null;

        switch (clipName)
        {
            case "Death": clip = deathSound; break;
            case "NewRecord": clip = newRecordSound; break;
            case "Fireball": clip = fireballWhoosh; break;
        }

        if (clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }
}