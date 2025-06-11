using UnityEngine;
using UnityEngine.UI;
using TMPro; // Assuming you are using TextMeshPro for text rendering

public class MuteButton : MonoBehaviour
{
    private Button button;
    private TMP_Text buttonText;

    void Start()
    {
        button = GetComponent<Button>();
        buttonText = GetComponentInChildren<TMP_Text>();

        button.onClick.AddListener(ToggleAudio);
        UpdateText();
    }

    void ToggleAudio()
    {
        AudioManager.Instance.ToggleMute();
        UpdateText();
    }

    void UpdateText()
    {
        buttonText.text = AudioManager.Instance.IsMuted() ? "Unmute" : "Mute";
    }
}
