using UnityEngine;
using UnityEngine.UI;

public class MuteButton : MonoBehaviour
{
    private Button button;
    private Text buttonText;

    void Start()
    {
        button = GetComponent<Button>();
        buttonText = GetComponentInChildren<Text>();

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
