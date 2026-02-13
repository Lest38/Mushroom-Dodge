using UnityEngine;

public class TestMusic : MonoBehaviour
{
    public AudioClip testClip;
    private AudioSource source;

    void Start()
    {
        // янгдюел AUDIOSOURCE
        source = gameObject.AddComponent<AudioSource>();
        source.clip = testClip;
        source.loop = true;
        source.volume = 1f;

        // опнасел бйкчвхрэ
        source.Play();

        Debug.Log("=== реяр лсгшйх ===");
        Debug.Log($"Clip: {testClip != null}");
        Debug.Log($"Source: {source != null}");
        Debug.Log($"IsPlaying: {source.isPlaying}");
    }
}