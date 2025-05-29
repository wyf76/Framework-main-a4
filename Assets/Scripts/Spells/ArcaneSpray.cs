using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

public sealed class ArcaneSpray : Spell
{
    // JSON-loaded spell attributes
    private string _displayName;
    private string _description;
    private int _iconIndex;
    private float _baseManaCost;
    private float _baseCooldownTime;
    private string _trajectory;
    private int _projectileSprite;

    // RPN expressions
    private string _damageExpression;
    private string _speedExpression;
    private string _lifetimeExpression;
    private string _countExpression;
    private float _sprayAngleDegrees;

    public ArcaneSpray(SpellCaster owner) : base(owner) { }

    public override string DisplayName => _displayName;
    public override int IconIndex => _iconIndex;

    protected override float BaseDamage => RPNEvaluator.SafeEvaluateFloat(_damageExpression, GetVars(), 3f);
    protected override float BaseSpeed => RPNEvaluator.SafeEvaluateFloat(_speedExpression, GetVars(), 8f);
    protected override float BaseMana => _baseManaCost;
    protected override float BaseCooldown => _baseCooldownTime;

    // Gather variables for RPN
    private Dictionary<string, float> GetVars() => new Dictionary<string, float>
    {
        ["power"] = owner.spellPower,
        ["wave"]  = GetCurrentWave()
    };

    private float GetCurrentWave()
    {
        var spawner = Object.FindFirstObjectByType<EnemySpawnerController>();
        return spawner != null ? spawner.currentWave : 1f;
    }

    public override void LoadAttributes(JObject json, Dictionary<string, float> initialVars)
    {
        // Identity
        _displayName = json["name"].Value<string>();
        _description = json["description"]?.Value<string>() ?? string.Empty;
        _iconIndex   = json["icon"].Value<int>();

        // Expressions
        _damageExpression   = json["damage"]["amount"].Value<string>();
        _speedExpression    = json["projectile"]["speed"].Value<string>();
        _lifetimeExpression = json["projectile"]["lifetime"].Value<string>();
        _countExpression    = json["N"]?.Value<string>() ?? "7";

        // Mana & cooldown
        _baseManaCost     = RPNEvaluator.SafeEvaluateFloat(json["mana_cost"].Value<string>(), initialVars, 1f);
        _baseCooldownTime = RPNEvaluator.SafeEvaluateFloat(json["cooldown"].Value<string>(), initialVars, 0.5f);

        // Visuals
        _trajectory            = json["projectile"]["trajectory"].Value<string>();
        _projectileSprite      = json["projectile"]["sprite"]?.Value<int>() ?? 0;
        _sprayAngleDegrees     = json["spray"] != null
            ? float.Parse(json["spray"].Value<string>()) * 180f
            : 60f;
    }

    // Cast the spray pattern
    protected override IEnumerator Cast(Vector3 origin, Vector3 targetPosition)
    {
        int count    = Mathf.RoundToInt(RPNEvaluator.SafeEvaluateFloat(_countExpression, GetVars(), 7f));
        float lifetime = RPNEvaluator.SafeEvaluateFloat(_lifetimeExpression, GetVars(), 0.5f);

        float damageValue = Damage;
        float speedValue  = Speed;

        Debug.Log($"[{_displayName}] Spray ▶ dmg={damageValue:F1}, spd={speedValue:F1}, " +
                  $"lifetime={lifetime:F2}, count={count}");

        Vector3 direction = (targetPosition - origin).normalized;
        float baseAngle  = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        float angleStep  = _sprayAngleDegrees / (count - 1);
        float startAngle = baseAngle - _sprayAngleDegrees / 2;

        for (int i = 0; i < count; i++)
        {
            float currentAngle   = startAngle + i * angleStep;
            Vector3 projDirection = new Vector3(Mathf.Cos(currentAngle * Mathf.Deg2Rad), Mathf.Sin(currentAngle * Mathf.Deg2Rad), 0f);

            GameManager.Instance.projectileManager.CreateProjectile(
                _projectileSprite,
                _trajectory,
                origin,
                projDirection,
                speedValue,
                (hit, _) =>
                {
                    if (hit.team != owner.team)
                    {
                        int amt = Mathf.RoundToInt(damageValue);
                        hit.Damage(new global::Damage(amt, global::Damage.Type.ARCANE));
                        Debug.Log($"[{_displayName}] Hit {hit.owner.name} for {amt}");
                    }
                },
                lifetime);

            yield return new WaitForSeconds(0.02f);
        }

        yield return null;
    }
}