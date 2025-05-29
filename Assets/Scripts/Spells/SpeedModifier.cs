using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

// Increases the speed of an inner spell temporarily.

public sealed class SpeedModifier : ModifierSpell
{
    private float _speedMultiplier = 1.75f;
    private string _modifierName   = "speed-amplified";

    public SpeedModifier(Spell inner) : base(inner) { }

    protected override string Suffix => _modifierName;

    public override void LoadAttributes(JObject json, Dictionary<string, float> vars)
    {
        _modifierName = json["name"]?.Value<string>() ?? _modifierName;
        if (json["speed_multiplier"] != null)
        {
            _speedMultiplier = RPNEvaluator.SafeEvaluateFloat(
                json["speed_multiplier"].Value<string>(), vars, _speedMultiplier);
        }
        base.LoadAttributes(json, vars);
    }

    protected override void InjectMods(StatBlock mods)
    {
        mods.SpeedMods.Add(new ValueMod(ModOp.Mul, _speedMultiplier));
    }

    protected override IEnumerator Cast(Vector3 from, Vector3 to)
    {
        // Temporarily merge our speed mods into the base spell
        Spell leaf = inner;
        while (leaf is ModifierSpell ms) leaf = ms.InnerSpell;

        var originalMods = leaf.mods;
        var merged = new StatBlock();

        // Copy existing and ours
        merged.DamageMods.AddRange(originalMods.DamageMods);
        merged.ManaMods.AddRange(originalMods.ManaMods);
        merged.SpeedMods.AddRange(originalMods.SpeedMods);
        merged.CooldownMods.AddRange(originalMods.CooldownMods);
        merged.SpeedMods.Add(new ValueMod(ModOp.Mul, _speedMultiplier));

        leaf.mods = merged;
        yield return inner.TryCast(from, to);
        leaf.mods = originalMods;
    }
}