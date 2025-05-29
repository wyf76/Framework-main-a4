using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;


// Splits an inner spell into two angled casts.
public sealed class Splitter : ModifierSpell
{
    private float _angle = 10f;
    private float _manaMultiplier = 1.5f;
    private string _modifierName = "split";

    public Splitter(Spell inner) : base(inner) { }

    protected override string Suffix => _modifierName;

    public override void LoadAttributes(JObject json, Dictionary<string, float> vars)
    {
        _modifierName = json["name"]?.Value<string>() ?? _modifierName;
        if (json["angle"] != null)
            _angle = RPNEvaluator.SafeEvaluateFloat(json["angle"].Value<string>(), vars, _angle);
        if (json["mana_multiplier"] != null)
            _manaMultiplier = RPNEvaluator.SafeEvaluateFloat(json["mana_multiplier"].Value<string>(), vars, _manaMultiplier);

        base.LoadAttributes(json, vars);
    }

    protected override void InjectMods(StatBlock mods) => mods.ManaMods.Add(new ValueMod(ModOp.Mul, _manaMultiplier));

    protected override IEnumerator ApplyModifierEffect(Vector3 from, Vector3 to)
    {
        Vector3 dir = (to - from).normalized;
        float baseDeg = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        
        // Left cast
        yield return CastAtAngle(from, baseDeg + _angle);
        // Right cast
        yield return CastAtAngle(from, baseDeg - _angle);
    }

    private IEnumerator CastAtAngle(Vector3 origin, float degree)
    {
        Vector3 d = new Vector3(Mathf.Cos(degree*Mathf.Deg2Rad), Mathf.Sin(degree*Mathf.Deg2Rad),0);
        yield return inner.TryCast(origin, origin + d * 10f);
    }
}
