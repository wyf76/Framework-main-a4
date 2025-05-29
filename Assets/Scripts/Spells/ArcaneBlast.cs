using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

public sealed class ArcaneBlast : Spell
{
    // JSON-loaded spell metadata
    private string _displayName;
    private string _description;
    private int _iconIndex;
    private float _baseManaCost;
    private float _baseCooldownTime;
    private string _primaryTrajectory;
    private int _primaryProjectileSprite;

    // RPN expressions for dynamic stats
    private string _damageExpression;
    private string _speedExpression;
    private string _secondaryDamageExpression;
    private string _secondaryCountExpression;

    // Secondary projectile settings
    private string _secondaryTrajectory;
    private float _secondarySpeed;
    private float _secondaryLifetime;
    private int _secondaryProjectileSprite;

    public ArcaneBlast(SpellCaster owner) : base(owner) { }

    public override string DisplayName => _displayName;
    public override int IconIndex => _iconIndex;

    protected override float BaseDamage =>
        RPNEvaluator.SafeEvaluateFloat(_damageExpression, GetEvaluationVariables(), 20f);

    protected override float BaseSpeed =>
        RPNEvaluator.SafeEvaluateFloat(_speedExpression, GetEvaluationVariables(), 12f);

    protected override float BaseMana => _baseManaCost;
    protected override float BaseCooldown => _baseCooldownTime;

    // Collect variables for RPN evaluation
    private Dictionary<string, float> GetEvaluationVariables() => new Dictionary<string, float>
    {
        ["power"] = owner.spellPower,
        ["wave"]  = GetCurrentWave()
    };

    // Retrieve current wave from EnemySpawner, defaulting to 1
    private float GetCurrentWave()
    {
        var spawner = Object.FindFirstObjectByType<EnemySpawnerController>();
        return spawner != null ? spawner.currentWave : 1f;
    }

    // Load JSON attributes into fields
    public override void LoadAttributes(JObject json, Dictionary<string, float> initialVars)
    {
        // Basic metadata
        _displayName = json["name"].Value<string>();
        _description = json["description"]?.Value<string>() ?? string.Empty;
        _iconIndex  = json["icon"].Value<int>();

        // Primary stats
        _damageExpression = json["damage"]["amount"].Value<string>();
        _speedExpression  = json["projectile"]["speed"].Value<string>();
        _baseManaCost     = RPNEvaluator.SafeEvaluateFloat(json["mana_cost"].Value<string>(), initialVars, 1f);
        _baseCooldownTime = RPNEvaluator.SafeEvaluateFloat(json["cooldown"].Value<string>(), initialVars, 0f);

        // Primary projectile behavior
        _primaryTrajectory       = json["projectile"]["trajectory"].Value<string>();
        _primaryProjectileSprite = json["projectile"]["sprite"]?.Value<int>() ?? 0;

        // Secondary settings
        _secondaryCountExpression  = json["N"]?.Value<string>();
        _secondaryDamageExpression = json["secondary_damage"]?.Value<string>();

        if (json["secondary_projectile"] != null)
        {
            var sec = json["secondary_projectile"];
            _secondaryTrajectory      = sec["trajectory"]?.Value<string>() ?? _primaryTrajectory;
            _secondarySpeed           = sec["speed"] != null
                ? RPNEvaluator.SafeEvaluateFloat(sec["speed"].Value<string>(), initialVars, BaseSpeed * 0.8f)
                : BaseSpeed * 0.8f;
            _secondaryLifetime        = sec["lifetime"] != null
                ? float.Parse(sec["lifetime"].Value<string>())
                : 0.3f;
            _secondaryProjectileSprite = sec["sprite"]?.Value<int>() ?? _primaryProjectileSprite;
        }
        else
        {
            _secondaryTrajectory       = _primaryTrajectory;
            _secondarySpeed            = BaseSpeed * 0.8f;
            _secondaryLifetime         = 0.3f;
            _secondaryProjectileSprite = _primaryProjectileSprite;
        }
    }

    // Cast the spell: primary and secondary projectiles
    protected override IEnumerator Cast(Vector3 origin, Vector3 targetPosition)
    {
        // Evaluate final values once at cast time
        float primaryDamage = Damage;
        float secondaryDamage = !string.IsNullOrEmpty(_secondaryDamageExpression)
            ? RPNEvaluator.SafeEvaluateFloat(_secondaryDamageExpression, GetEvaluationVariables(), primaryDamage * 0.25f)
            : primaryDamage * 0.25f;
        int secondaryCount = !string.IsNullOrEmpty(_secondaryCountExpression)
            ? Mathf.RoundToInt(RPNEvaluator.SafeEvaluateFloat(_secondaryCountExpression, GetEvaluationVariables(), 8f))
            : 8;

        Debug.Log($"[{_displayName}] Casting: primaryDamage={primaryDamage:F1}, speed={Speed:F1}," +
                  $" secondaryDamage={secondaryDamage:F1} x{secondaryCount}");

        // Launch primary projectile
        GameManager.Instance.projectileManager.CreateProjectile(
            _primaryProjectileSprite,
            _primaryTrajectory,
            origin,
            targetPosition - origin,
            Speed,
            (hit, impactPos) =>
            {
                if (hit.team != owner.team)
                {
                    int damageAmount = Mathf.RoundToInt(primaryDamage);
                    hit.Damage(new global::Damage(damageAmount, global::Damage.Type.ARCANE));
                    Debug.Log($"[{_displayName}] Primary hit {hit.owner.name} for {damageAmount}");

                    // Spawn secondaries
                    SpawnSecondaryProjectiles(impactPos, secondaryDamage, secondaryCount);
                }
            });

        yield return null;
    }

    // Spawn secondary projectiles in circle
    private void SpawnSecondaryProjectiles(Vector3 center, float damage, int count)
    {
        float angleStep = 360f / count;

        for (int i = 0; i < count; i++)
        {
            float angleRad = (i * angleStep) * Mathf.Deg2Rad;
            Vector3 direction = new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad), 0f).normalized;

            GameManager.Instance.projectileManager.CreateProjectile(
                _secondaryProjectileSprite,
                _secondaryTrajectory,
                center,
                direction,
                _secondarySpeed * Speed / BaseSpeed,
                (hit, _) =>
                {
                    if (hit.team != owner.team)
                    {
                        int dmg = Mathf.RoundToInt(damage);
                        hit.Damage(new global::Damage(dmg, global::Damage.Type.ARCANE));
                        Debug.Log($"[{_displayName}] Secondary hit {hit.owner.name} for {dmg}");
                    }
                },
                _secondaryLifetime);
        }
    }
}