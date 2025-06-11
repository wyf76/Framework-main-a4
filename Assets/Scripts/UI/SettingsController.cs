using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsController : MonoBehaviour
{
    [Header("Audio")]
    public AudioMixer mixer;            // assign your mixer here
    public Slider musicSlider;          // hook this up in the inspector

    void Start() {
        // initialize slider to current saved volume (or 0.75 by default)
        float saved = PlayerPrefs.GetFloat("MusicVolume", 0.75f);
        musicSlider.value = saved;
        ApplyMusicVolume(saved);

        // subscribe so runtime changes apply immediately
        musicSlider.onValueChanged.AddListener(ApplyMusicVolume);
    }

    public void ApplyMusicVolume(float sliderValue) {
        // convert [0..1] slider into decibels:
        mixer.SetFloat("MusicVol", Mathf.Log10(Mathf.Clamp(sliderValue, 0.0001f, 1f)) * 20f);
        PlayerPrefs.SetFloat("MusicVolume", sliderValue);
    }
}
