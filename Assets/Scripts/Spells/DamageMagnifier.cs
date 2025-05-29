// File: Assets/Scripts/Spells/DamageMagnifier.cs
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

public sealed class DamageMagnifier : ModifierSpell
{
    private float damageMultiplier = 1.5f;
    private float manaMultiplier = 1.5f;
    private string modifierName = "damage-amplified";

    public DamageMagnifier(Spell inner) : base(inner) { }

    protected override string Suffix => modifierName;

    public override void LoadAttributes(JObject j, Dictionary<string, float> vars)
    {
        modifierName = j["name"]?.Value<string>() ?? modifierName;
        if (j["damage_multiplier"] != null)
            damageMultiplier = RPNEvaluator.SafeEvaluateFloat(
                j["damage_multiplier"].Value<string>(),
                vars,
                damageMultiplier
            );
        if (j["mana_multiplier"] != null)
            manaMultiplier = RPNEvaluator.SafeEvaluateFloat(
                j["mana_multiplier"].Value<string>(),
                vars,
                manaMultiplier
            );
        base.LoadAttributes(j, vars);
    }

    protected override void InjectMods(StatBlock mods)
    {
        mods.DamageMods.Add(new ValueMod(ModOp.Mul, damageMultiplier));
        mods.ManaMods.Add(new ValueMod(ModOp.Mul, manaMultiplier));
    }

    protected override IEnumerator Cast(Vector3 from, Vector3 to)
    {
        // 1) Find the true “leaf” spell
        Spell leaf = inner;
        while (leaf is ModifierSpell ms) leaf = ms.InnerSpell;

        // 2) Save original mods
        var original = leaf.mods;

        // 3) Merge original + our damage/mana mods
        var merged = new StatBlock();
        // copy all lists
        foreach (var m in original.DamageMods) merged.DamageMods.Add(m);
        foreach (var m in original.ManaMods) merged.ManaMods.Add(m);
        foreach (var m in original.SpeedMods) merged.SpeedMods.Add(m);
        foreach (var m in original.CooldownMods) merged.CooldownMods.Add(m);
        // add ours
        foreach (var m in this.mods.DamageMods) merged.DamageMods.Add(m);
        foreach (var m in this.mods.ManaMods) merged.ManaMods.Add(m);
        foreach (var m in this.mods.SpeedMods) merged.SpeedMods.Add(m);
        foreach (var m in this.mods.CooldownMods) merged.CooldownMods.Add(m);

        leaf.mods = merged;

        // 4) Run the full chain under combined mods
        yield return inner.TryCast(from, to);

        // 5) Restore
        leaf.mods = original;
    }
}
