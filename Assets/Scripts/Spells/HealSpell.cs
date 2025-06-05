using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

public sealed class HealSpell : Spell
{
    private string _displayName = "Heal";
    private int _iconIndex = 0;
    private string _healExpr = "10";
    private float _baseMana = 5f;
    private float _baseCooldown = 5f;
    private float _range = 5f;

    public HealSpell(SpellCaster owner) : base(owner) { }

    public override string DisplayName => _displayName;
    public override int IconIndex => _iconIndex;

    protected override float BaseDamage => 0f; // unused
    protected override float BaseMana => _baseMana;
    protected override float BaseCooldown => _baseCooldown;
    protected override float BaseSpeed => 0f;

    public override void LoadAttributes(JObject json, Dictionary<string,float> vars)
    {
        _displayName = json["name"]?.Value<string>() ?? _displayName;
        _iconIndex   = json["icon"]?.Value<int>() ?? _iconIndex;
        _healExpr    = json["heal_amount"]?.Value<string>() ?? _healExpr;
        _baseMana    = RPNEvaluator.SafeEvaluateFloat(json["mana_cost"]?.Value<string>(), vars, _baseMana);
        _baseCooldown= RPNEvaluator.SafeEvaluateFloat(json["cooldown"]?.Value<string>(), vars, _baseCooldown);
        _range       = RPNEvaluator.SafeEvaluateFloat(json["range"]?.Value<string>(), vars, _range);
    }

    protected override IEnumerator Cast(Vector3 origin, Vector3 target)
    {
        float val = RPNEvaluator.SafeEvaluateFloat(_healExpr,
            new Dictionary<string,float>{
                ["power"] = owner.spellPower,
                ["wave"]  = Object.FindFirstObjectByType<EnemySpawnerController>()?.currentWave ?? 1
            },
            5f);

        var enemies = Object.FindObjectsByType<EnemyController>(FindObjectsSortMode.None);
        EnemyController best = null;
        float bestDist = float.MaxValue;
        foreach(var e in enemies)
        {
            if(e.hp.hp >= e.hp.max_hp) continue;
            float d = Vector3.Distance(e.transform.position, origin);
            if(d <= _range && d < bestDist)
            {
                best = e;
                bestDist = d;
            }
        }
        if(best == null)
            best = owner.GetComponent<EnemyController>();

        best.hp.Heal(Mathf.RoundToInt(val));
        yield return null;
    }
}
