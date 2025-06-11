using UnityEngine;
using TMPro;

public class MuteButton : MonoBehaviour
{
    [SerializeField] 
    private TextMeshProUGUI muteButtonText;  

    private void Awake()
    {
        if (muteButtonText == null)
        {
            muteButtonText = GetComponentInChildren<TextMeshProUGUI>();
            if (muteButtonText == null)
                Debug.LogError($"[{name}] MuteButton: no TextMeshProUGUI found!");
        }
    }

    private void Start()
    {
        UpdateText();
    }

    public void ToggleMute()
    {
        AudioListener.volume = (AudioListener.volume == 0f) ? 1f : 0f;
        UpdateText();
    }

    private void UpdateText()
    {
        if (muteButtonText == null) return;
        muteButtonText.text = (AudioListener.volume == 0f) ? "Unmute" : "Mute";
    }
}
