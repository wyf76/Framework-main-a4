using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    private AudioSource audioSource;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            audioSource = GetComponent<AudioSource>();
            audioSource.Play();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ToggleMute()
    {
        audioSource.mute = !audioSource.mute;
    }

    public bool IsMuted()
    {
        return audioSource.mute;
    }
}
