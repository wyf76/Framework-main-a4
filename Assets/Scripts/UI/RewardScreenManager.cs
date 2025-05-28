using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class RewardScreenManager : MonoBehaviour
{
    public static RewardScreenManager Instance;

    public GameObject rewardUI;
    public TextMeshProUGUI buttonText;

    public PlayerController player;
    public SpellUIContainer spellUIContainer;

    // ─────────────────────────────────────────────────────────────
    // REMOVED: relicReward flag, relicChoices and all key‑input code
    // ─────────────────────────────────────────────────────────────

    private Spell rewardSpell;

    private void Awake() => Instance = this;

    private void Start() => rewardUI.SetActive(false);

    private void Update()
    {
        if (GameManager.Instance.state is GameManager.GameState.WAVEEND
                                         or GameManager.GameState.GAMEOVER)
        {
            rewardUI.SetActive(true);
            buttonText.text = GameManager.Instance.state == GameManager.GameState.GAMEOVER
                              ? "Return to Menu"
                              : buttonText.text;
        }
        else
        {
            rewardUI.SetActive(false);
        }
    }

    public void ShowReward()
    {
        rewardUI.SetActive(true);
        Debug.Log("Reward screen activated");

        // reset any previous reward state
        rewardSpell = null;

        // the wave that just ended
        int completedWave = GameManager.Instance.currentWave - 1;

        /*  Drop three relics every third completed wave, starting with wave 3
        *  (i.e., after completing waves 3, 6, 9, …)
        */
        bool relicReward = (completedWave + 1) % 3 == 0;

        if (relicReward)
        {
            // Build three unique relics the player doesn’t own
            List<string> available = new List<string>(RelicManager.Relics.Keys);
            foreach (Relic r in player.relics) available.Remove(r.name);

            List<Relic> relicChoices = new List<Relic>();
            for (int i = 0; i < 3 && available.Count > 0; i++)
            {
                int idx    = Random.Range(0, available.Count);
                string key = available[idx];
                available.RemoveAt(idx);
                relicChoices.Add(RelicManager.BuildRelic(key));
            }

            // Hand choices off to the stat/text manager for display & input
            PlayerStatTextManager.Instance.ShowRelicChoices(relicChoices, player);
            buttonText.text = "Press 1 / 2 / 3";
            return;
        }

        // Otherwise give a spell reward
        rewardSpell = new SpellBuilder().Build(player.spellcaster);
        Debug.Log($"New spell reward: {rewardSpell.GetName()} (Mana: {rewardSpell.GetManaCost()}, Damage: {rewardSpell.GetDamage()})");
        PlayerStatTextManager.Instance.SetRewardMessage($"NEW SPELL: {rewardSpell.GetName()}");
        buttonText.text = "Accept Spell";
    }

    public void AcceptReward()
    {
        // only fires for the spell path now
        if (rewardSpell == null) return;

        player.spellcaster.spell = rewardSpell;
        spellUIContainer.spellUIs[0].GetComponent<SpellUI>()
                                   .SetSpell(rewardSpell);

        PlayerStatTextManager.Instance.SetRewardMessage(
            $"EQUIPPED SPELL: {rewardSpell.GetName()}");

        rewardUI.SetActive(false);
        GameManager.Instance.NextWave();
    }

    // called by PlayerStatTextManager when a relic is picked
    public void CloseAndNextWave()              // <──── new helper
    {
        rewardUI.SetActive(false);
        GameManager.Instance.NextWave();
    }
}
