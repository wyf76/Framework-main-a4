using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

public sealed class SpeedBuffSpell : Spell
{
    private string _displayName = "Haste";
    private int _iconIndex = 0;
    private string _durationExpr = "3";
    private string _multiplierExpr = "1.5";
    private float _baseMana = 5f;
    private float _baseCooldown = 5f;
    private float _range = 5f;

    public SpeedBuffSpell(SpellCaster owner) : base(owner) { }

    public override string DisplayName => _displayName;
    public override int IconIndex => _iconIndex;

    protected override float BaseDamage => 0f;
    protected override float BaseMana => _baseMana;
    protected override float BaseCooldown => _baseCooldown;
    protected override float BaseSpeed => 0f;

    public override void LoadAttributes(JObject json, Dictionary<string,float> vars)
    {
        _displayName = json["name"]?.Value<string>() ?? _displayName;
        _iconIndex   = json["icon"]?.Value<int>() ?? _iconIndex;
        _durationExpr   = json["duration"]?.Value<string>() ?? _durationExpr;
        _multiplierExpr = json["multiplier"]?.Value<string>() ?? _multiplierExpr;
        _baseMana    = RPNEvaluator.SafeEvaluateFloat(json["mana_cost"]?.Value<string>(), vars, _baseMana);
        _baseCooldown= RPNEvaluator.SafeEvaluateFloat(json["cooldown"]?.Value<string>(), vars, _baseCooldown);
        _range       = RPNEvaluator.SafeEvaluateFloat(json["range"]?.Value<string>(), vars, _range);
    }

    protected override IEnumerator Cast(Vector3 origin, Vector3 target)
    {
        float duration = RPNEvaluator.SafeEvaluateFloat(_durationExpr,
            new Dictionary<string,float>{
                ["power"] = owner.spellPower,
                ["wave"]  = Object.FindFirstObjectByType<EnemySpawnerController>()?.currentWave ?? 1
            },
            3f);
        float mult = RPNEvaluator.SafeEvaluateFloat(_multiplierExpr,
            new Dictionary<string,float>{{"power",owner.spellPower},{"wave",Object.FindFirstObjectByType<EnemySpawnerController>()?.currentWave ?? 1}},
            1.5f);

        var enemies = Object.FindObjectsByType<EnemyController>(FindObjectsSortMode.None);
        foreach(var e in enemies)
        {
            if(Vector3.Distance(e.transform.position, origin) <= _range)
                e.ApplySpeedBuff(duration, mult);
        }
        yield return null;
    }
}
