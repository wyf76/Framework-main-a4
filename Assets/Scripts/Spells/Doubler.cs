using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

public sealed class Doubler : ModifierSpell
{
    private float _delay;
    private float _manaMultiplier;
    private float _cdMultiplier;
    private string _suffix;

    public Doubler(Spell inner) : base(inner)
    {
        _delay           = 0.5f;
        _manaMultiplier  = 1.5f;
        _cdMultiplier    = 1.5f;
        _suffix          = "doubled";
    }

    protected override string Suffix => _suffix;

    public override void LoadAttributes(JObject json, Dictionary<string, float> vars)
    {
        _suffix = json["name"]?.Value<string>() ?? _suffix;

        if (json["delay"] != null)
            _delay = RPNEvaluator.SafeEvaluateFloat(json["delay"].Value<string>(), vars, _delay);

        if (json["mana_multiplier"] != null)
            _manaMultiplier = RPNEvaluator.SafeEvaluateFloat(json["mana_multiplier"].Value<string>(), vars, _manaMultiplier);

        if (json["cooldown_multiplier"] != null)
            _cdMultiplier = RPNEvaluator.SafeEvaluateFloat(json["cooldown_multiplier"].Value<string>(), vars, _cdMultiplier);

        base.LoadAttributes(json, vars);
    }

    protected override void InjectMods(StatBlock mods)
    {
        mods.ManaMods.Add(new ValueMod(ModOp.Mul, _manaMultiplier));
        mods.CooldownMods.Add(new ValueMod(ModOp.Mul, _cdMultiplier));
    }

    protected override IEnumerator Cast(Vector3 origin, Vector3 target)
    {
        // First cast
        yield return inner.TryCast(origin, target);

        // Delay
        yield return new WaitForSeconds(_delay);

        // Recompute direction from current position
        Vector3 start = owner.transform.position;
        Vector3 dir   = (target - origin).normalized;
        Vector3 end   = start + dir * Vector3.Distance(origin, target);

        // Second cast
        yield return inner.TryCast(start, end);
    }
}
