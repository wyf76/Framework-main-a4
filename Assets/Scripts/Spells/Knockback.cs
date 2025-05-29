using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

// Base spell that launches a projectile and applies knockback on hit.

public sealed class KnockbackSpell : Spell
{
    private string _displayName;
    private int    _iconIndex;
    private string _damageExpr;
    private float  _baseManaCost;
    private float  _baseCooldown;
    private string _trajectory;
    private string _speedExpr;
    private int    _projectileSprite;
    private string _forceExpr;

    public KnockbackSpell(SpellCaster owner) : base(owner) { }

    public override string DisplayName => _displayName;
    public override int    IconIndex   => _iconIndex;

    protected override float BaseDamage   =>
        RPNEvaluator.SafeEvaluateFloat(_damageExpr, GetVars(), 8f);
    protected override float BaseMana     => _baseManaCost;
    protected override float BaseCooldown => _baseCooldown;
    protected override float BaseSpeed    =>
        RPNEvaluator.SafeEvaluateFloat(_speedExpr, GetVars(), 10f);

    private Dictionary<string,float> GetVars() => new Dictionary<string,float>
    {
        ["power"] = owner.spellPower,
        ["wave"]  = Object.FindFirstObjectByType<EnemySpawnerController>()?.currentWave ?? 1
    };

    public override void LoadAttributes(JObject json, Dictionary<string,float> vars)
    {
        //— all your other field assignments up above
        _displayName      = json["name"]?.Value<string>()       ?? _displayName;
        _iconIndex        = json["icon"]?.Value<int>()          ?? _iconIndex;
        _damageExpr       = json["damage"]?["amount"]?.Value<string>() ?? _damageExpr;
        _baseManaCost     = RPNEvaluator.SafeEvaluateFloat(json["mana_cost"]?.Value<string>(),
                                                        vars,
                                                        _baseManaCost);
        _baseCooldown     = RPNEvaluator.SafeEvaluateFloat(json["cooldown"]?.Value<string>(),
                                                        vars,
                                                        _baseCooldown);
        _trajectory       = json["projectile"]?["trajectory"]?.Value<string>() ?? _trajectory;
        _speedExpr        = json["projectile"]?["speed"]?.Value<string>()      ?? _speedExpr;
        _projectileSprite = json["projectile"]?["sprite"]?.Value<int>()        ?? _projectileSprite;

        // —— NOW the safe, guarded effects read: ——
        var effects = json["effects"] as JArray;
        if (effects != null && effects.Count > 0)
        {
            // find the first “knockback” effect
            foreach (var e in effects)
            {
                if (e["type"]?.Value<string>() == "knockback")
                {
                    _forceExpr = e["force"]?.Value<string>() ?? _forceExpr;
                    break;
                }
            }
        }
        else
        {
            Debug.LogWarning($"[KnockbackSpell] no effects[] array found in JSON for '{_displayName}', defaulting force to '{_forceExpr}'");
        }
    }

    protected override IEnumerator Cast(Vector3 origin, Vector3 target)
    {
        float dmg   = Damage;
        float spd   = Speed;
        float force = 5f;
        if (!string.IsNullOrWhiteSpace(_forceExpr))
            force = RPNEvaluator.SafeEvaluateFloat(_forceExpr, GetVars(), force);
        
        GameManager.Instance.projectileManager.CreateProjectile(
            _projectileSprite,
            _trajectory,
            origin,
            (target - origin).normalized,
            spd,
            (hit, pos) =>
            {
                if (hit.team != owner.team)
                {
                    // Damage
                    hit.Damage(new global::Damage(Mathf.RoundToInt(dmg), global::Damage.Type.PHYSICAL));
                    
                    Debug.Log($"[Knockback] Impulse={force:F1} on {hit.owner.name}");

                    // Knockback
                    var rb = hit.owner.GetComponent<Rigidbody2D>();
                    if (rb != null && rb.bodyType == RigidbodyType2D.Dynamic)
                        rb.AddForce((hit.owner.transform.position - pos).normalized * force,
                                    ForceMode2D.Impulse);
                }
            });

        yield return null;
    }
}