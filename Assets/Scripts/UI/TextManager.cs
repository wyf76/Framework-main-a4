using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class PlayerStatTextManager : MonoBehaviour
{
    public static PlayerStatTextManager Instance;

    private TextMeshProUGUI _text;

    // ─────────────────────────────────────────────────────────────
    // NEW: relic‑selection state
    // ─────────────────────────────────────────────────────────────
    private List<Relic> _relicChoices;
    private PlayerController _player;
    private bool _awaitingRelicChoice;
    private string _rewardMsg = string.Empty;

    private void Awake()
    {
        Instance = this;
        _text    = GetComponent<TextMeshProUGUI>();
    }

    private void Start() => _text.text = string.Empty;

    // ─────────────────────────────────────────────────────────────
    // Called by RewardScreenManager right after building the list
    // ─────────────────────────────────────────────────────────────
    public void ShowRelicChoices(List<Relic> choices, PlayerController pc)
    {
        _relicChoices         = choices;
        _player               = pc;
        _awaitingRelicChoice  = true;

        string msg = "CHOOSE RELIC:\n";
        for (int i = 0; i < choices.Count; i++)
            msg += $"{i + 1}: {choices[i].name}\n";

        _rewardMsg = msg.TrimEnd();
    }

    public void SetRewardMessage(string msg) => _rewardMsg = msg;

    private void Update()
    {
        switch (GameManager.Instance.state)
        {
            case GameManager.GameState.WAVEEND:
            case GameManager.GameState.GAMEOVER:

                _text.text =
                    $"TOTAL TIME: {GameManager.Instance.timeSpent}\n" +
                    $"DAMAGE DEALT: {GameManager.Instance.damageDealt}\n" +
                    $"DAMAGE RECEIVED: {GameManager.Instance.damageReceived}";

                if (!string.IsNullOrEmpty(_rewardMsg))
                    _text.text += $"\n{_rewardMsg}";

                // ───────────────────────────────────────────────
                // Hotkeys 1–3 for relics, if waiting for choice
                // ───────────────────────────────────────────────
                if (_awaitingRelicChoice && _relicChoices != null)
                {
                    if (Input.GetKeyDown(KeyCode.Alpha1)) PickRelic(0);
                    if (Input.GetKeyDown(KeyCode.Alpha2)) PickRelic(1);
                    if (Input.GetKeyDown(KeyCode.Alpha3)) PickRelic(2);
                }
                break;

            default:
                if (_text.text.Length > 0) _text.text = string.Empty;
                // clear state for next wave
                _rewardMsg            = string.Empty;
                _awaitingRelicChoice  = false;
                _relicChoices         = null;
                _player               = null;
                break;
        }
    }

    private void PickRelic(int index)
    {
        if (_relicChoices == null || index < 0 || index >= _relicChoices.Count)
            return;

        Relic picked = _relicChoices[index];
        _player.AddRelic(picked);

        _rewardMsg           = $"PICKED RELIC: {picked.name}";
        _awaitingRelicChoice = false;

        Debug.Log($"Picked relic: {picked.name}");
        RewardScreenManager.Instance.CloseAndNextWave();
    }
}
