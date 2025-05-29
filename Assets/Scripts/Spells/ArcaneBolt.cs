using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

public sealed class ArcaneBolt : Spell
{
    // JSON-loaded fields
    private string _displayName;
    private Damage.Type _damageType;
    private float _baseManaCost;
    private float _baseCooldownTime;
    private int _iconIndex;
    private string _trajectory;
    private int _projectileSprite;

    // RPN expressions for dynamic scaling
    private string _damageExpression;
    private string _speedExpression;

    public ArcaneBolt(SpellCaster owner) : base(owner) { }

    public override string DisplayName => _displayName;
    public override int IconIndex => _iconIndex;

    protected override float BaseDamage => RPNEvaluator.SafeEvaluateFloat(
        _damageExpression,
        new Dictionary<string, float> {
            ["power"] = owner.spellPower,
            ["wave"]  = GetCurrentWave()
        },
        10f);

    protected override float BaseSpeed => RPNEvaluator.SafeEvaluateFloat(
        _speedExpression,
        new Dictionary<string, float> {
            ["power"] = owner.spellPower,
            ["wave"]  = GetCurrentWave()
        },
        8f);

    protected override float BaseMana => _baseManaCost;
    protected override float BaseCooldown => _baseCooldownTime;

    // Get current wave from spawner
    private float GetCurrentWave()
    {
        var spawner = UnityEngine.Object.FindFirstObjectByType<EnemySpawnerController>();
        return spawner != null ? spawner.currentWave : 1f;
    }

    // Load JSON attributes
    public override void LoadAttributes(JObject json, Dictionary<string, float> initialVars)
    {
        _displayName = json["name"].Value<string>();
        _iconIndex   = json["icon"].Value<int>();

        _damageExpression = json["damage"]["amount"].Value<string>();
        _damageType       = (Damage.Type)Enum.Parse(typeof(Damage.Type), json["damage"]["type"].Value<string>(), true);

        _baseManaCost     = RPNEvaluator.SafeEvaluateFloat(json["mana_cost"].Value<string>(), initialVars, 1f);
        _baseCooldownTime = RPNEvaluator.SafeEvaluateFloat(json["cooldown"].Value<string>(), initialVars, 0f);

        _trajectory       = json["projectile"]["trajectory"].Value<string>();
        _speedExpression  = json["projectile"]["speed"].Value<string>();
        _projectileSprite = json["projectile"]["sprite"].Value<int>();
    }

    // Cast the bolt
    protected override IEnumerator Cast(Vector3 origin, Vector3 targetPosition)
    {
        float damageValue = Damage;
        float speedValue  = Speed;

        Debug.Log($"[{_displayName}] Casting bolt ▶ dmg={damageValue:F1} ({_damageType}), mana={Mana:F1}, spd={speedValue:F1}");

        GameManager.Instance.projectileManager.CreateProjectile(
            _projectileSprite,
            _trajectory,
            origin,
            targetPosition - origin,
            speedValue,
            (hit, _) =>
            {
                if (hit.team != owner.team)
                {
                    int amt = Mathf.RoundToInt(damageValue);
                    hit.Damage(new global::Damage(amt, _damageType));
                    Debug.Log($"[{_displayName}] Hit {hit.owner.name} for {amt} ({_damageType})");
                }
            });

        yield return null;
    }
}