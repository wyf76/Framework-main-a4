using UnityEngine;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;



public class SpellBuilder
{
    private Dictionary<string, JObject> spellDefinitions = new();
    private List<string> baseSpellNames = new();
    private List<string> modifierNames = new();


    public SpellBuilder()
    {
        var spellText = Resources.Load<TextAsset>("spells");
        if (spellText == null)
        {
            Debug.LogError("spells.json not found in Resources.");
            return;
        }

        JObject root = JObject.Parse(spellText.text);

        foreach (var pair in root)
        {
            string id = pair.Key;
            JObject def = (JObject)pair.Value;

            bool isBase = def["damage"] != null && def["projectile"] != null;

            spellDefinitions[id] = def;

            if (isBase)
                baseSpellNames.Add(id);
            else
                modifierNames.Add(id);
        }
    }

    public Spell Build(SpellCaster owner)
    {
        // Picks a random base spell
        string baseName = baseSpellNames[Random.Range(0, baseSpellNames.Count)];
        JObject baseDef = spellDefinitions[baseName];

        BaseSpell baseSpell;

        if (baseName == "arcane_chain")
        {
            baseSpell = new ArcaneChainSpell(owner);
        }
        else
        {
            baseSpell = new BaseSpell(owner);
        }

        baseSpell.name = baseDef["name"]?.ToString();
        baseSpell.description = baseDef["description"]?.ToString();
        baseSpell.icon = baseDef["icon"]?.ToObject<int>() ?? 0;
        baseSpell.damageExpression = baseDef["damage"]?["amount"]?.ToString();
        baseSpell.manaCostExpression = baseDef["mana_cost"]?.ToString();
        baseSpell.cooldownExpression = baseDef["cooldown"]?.ToString();
        baseSpell.projectileSpeedExpression = baseDef["projectile"]?["speed"]?.ToString();
        baseSpell.trajectory = baseDef["projectile"]?["trajectory"]?.ToString();
        baseSpell.projectileSprite = baseDef["projectile"]?["sprite"]?.ToObject<int>() ?? 0;

        Spell finalSpell = baseSpell;

        // 50% chance to apply SplitterSpell
        if (Random.value < 0.5f && spellDefinitions.ContainsKey("splitter"))
        {
            JObject splitDef = spellDefinitions["splitter"];
            float angle = EvalFloat(splitDef["angle"]?.ToString(), owner);
            finalSpell = new SplitterSpell(owner, finalSpell, angle);
            Debug.Log("→ Applied SplitterSpell");
        }

        // 25% chance to apply DoublerSpell
        if (Random.value < 0.25f && spellDefinitions.ContainsKey("doubler"))
        {
            JObject doubleDef = spellDefinitions["doubler"];
            float delay = EvalFloat(doubleDef["delay"]?.ToString(), owner);
            finalSpell = new DoublerSpell(owner, finalSpell, delay);
            Debug.Log("→ Applied DoublerSpell");
        }

        // Apply 0–2 stat-based or behavior modifier layers
        int modifierCount = Random.Range(0, 3);

        for (int i = 0; i < modifierCount; i++)
        {
            string modName = modifierNames[Random.Range(0, modifierNames.Count)];
            if (modName == "splitter" || modName == "doubler") continue; // already applied

            JObject modDef = spellDefinitions[modName];

            // Handle custom behavior modifier: Piercing
            if (modName == "piercing")
            {
                finalSpell = new PiercingSpell(owner, finalSpell);
                Debug.Log("→ Applied PiercingSpell");
                continue;
            }

            // Handle custom behavior modifier: CheapCast
            if (modName == "cheap_cast")
            {
                finalSpell = new CheapCastSpell(owner, finalSpell);
                Debug.Log("→ Applied CheapCastSpell");
                continue;
            }

            // Default stat-based modifier
            ModifierSpell mod = new ModifierSpell(owner, finalSpell);

            if (modDef["damage_multiplier"] != null)
                mod.SetDamageMultiplier(EvalFloat(modDef["damage_multiplier"].ToString(), owner));

            if (modDef["mana_multiplier"] != null)
                mod.SetManaMultiplier(EvalFloat(modDef["mana_multiplier"].ToString(), owner));

            if (modDef["cooldown_multiplier"] != null)
                mod.SetCooldownMultiplier(EvalFloat(modDef["cooldown_multiplier"].ToString(), owner));

            if (modDef["speed_multiplier"] != null)
                mod.SetSpeedMultiplier(EvalFloat(modDef["speed_multiplier"].ToString(), owner));

            if (modDef["mana_adder"] != null)
                mod.SetManaAdder(EvalFloat(modDef["mana_adder"].ToString(), owner));

            if (modDef["projectile_trajectory"] != null)
                mod.SetOverrideTrajectory(modDef["projectile_trajectory"].ToString());

            finalSpell = mod;
            Debug.Log($"→ Applied stat modifier: {modName}");
        }
        Debug.Log($"Built spell: {finalSpell.GetName()} (damage: {finalSpell.GetDamage()}, mana: {finalSpell.GetManaCost()})");
        return finalSpell;
    }

    private float EvalFloat(string expr, SpellCaster owner)
    {
        if (string.IsNullOrEmpty(expr)) return 0f;
        return RPNEvaluator.Evaluate(expr, new Dictionary<string, float>
        {
            { "power", owner.spellPower },
            { "wave", GameManager.Instance.currentWave }
        });
    }
}