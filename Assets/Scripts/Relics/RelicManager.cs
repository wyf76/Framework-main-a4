using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

/// <summary>
/// Handles loading relic definitions from JSON and building concrete Relic instances at runtime.
/// </summary>
public class RelicManager : MonoBehaviour
{
    public static Dictionary<string, RelicDefinition> Relics;

    /// <summary>
    /// Loads relic definitions from Resources/relics.json once and caches them.
    /// </summary>
    private static void LoadRelics()
    {
        if (Relics != null) return; // Already loaded

        TextAsset relicText = Resources.Load<TextAsset>("relics");
        if (relicText == null)
        {
            Debug.LogError("relics.json not found in Resources");
            Relics = new Dictionary<string, RelicDefinition>();
            return;
        }

        JArray root = JArray.Parse(relicText.text);
        Relics = new Dictionary<string, RelicDefinition>();
        foreach (JObject obj in root)
        {
            var def = obj.ToObject<RelicDefinition>();
            if (def == null)
            {
                Debug.LogWarning("Failed to parse relic json node: " + obj.ToString());
                continue;
            }
            Relics[def.name] = def;
        }
    }

    private void Awake()
    {
        // Ensure relic definitions are loaded before anything else tries to use them.
        LoadRelics();
    }

    /// <summary>
    /// Builds a Relic by name. Returns null if the name is unknown.
    /// </summary>
    public static Relic BuildRelic(string name)
    {
        LoadRelics(); // Safety in case this gets called early.

        if (!Relics.TryGetValue(name, out RelicDefinition def))
        {
            Debug.LogWarning($"Relic '{name}' not found");
            return null;
        }

        var trigger = BuildTrigger(def.trigger);
        var effect = BuildEffect(def.effect);

        return new Relic(def.name, def.sprite, trigger, effect);
    }

    private static RelicTrigger BuildTrigger(RelicTriggerDefinition def)
    {
        switch (def.type)
        {
            case "take-damage":
                return new TakeDamageTrigger();

            case "stand-still":
                if (float.TryParse(def.amount, out float standTime))
                    return new StandStillTrigger(standTime);
                Debug.LogWarning($"Invalid amount '{def.amount}' for stand-still trigger");
                return new StandStillTrigger(0f);

            case "on-kill":
                return new KillTrigger();

            case "deal-damage":
                return new DealDamageTrigger();

            case "distance-moved":
                if (float.TryParse(def.amount, out float distance))
                    return new DistanceMovedTrigger(distance);
                Debug.LogWarning($"Invalid amount '{def.amount}' for distance-moved trigger");
                return new DistanceMovedTrigger(0f);

            default:
                Debug.LogWarning($"Unknown trigger type '{def.type}', defaulting to take-damage");
                return new TakeDamageTrigger();
        }
    }

    private static RelicEffect BuildEffect(RelicEffectDefinition def)
    {
        switch (def.type)
        {
            case "gain-mana":
                return new GainManaEffect(def.amount);

            case "gain-spellpower":
                return new GainSpellpowerEffect(def.amount, def.until);

            case "heal":
                return new HealEffect(def.amount);

            default:
                Debug.LogWarning($"Unknown effect type '{def.type}', defaulting to gain-mana");
                return new GainManaEffect(def.amount);
        }
    }
}

// ---------- JSON definition structs ----------

[System.Serializable]
public class RelicTriggerDefinition
{
    public string description;
    public string type;
    public string amount;
}

[System.Serializable]
public class RelicEffectDefinition
{
    public string description;
    public string type;
    public string amount;
    public string until;
}

[System.Serializable]
public class RelicDefinition
{
    public string name;
    public int sprite;
    public RelicTriggerDefinition trigger;
    public RelicEffectDefinition effect;
}
