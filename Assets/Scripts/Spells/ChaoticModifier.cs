using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

public sealed class ChaoticModifier : ModifierSpell
{
    // Modifier settings
    private float _damageMultiplier = 1.5f;
    private string _suffix = "chaotic";

    public ChaoticModifier(Spell inner) : base(inner) { }

    protected override string Suffix => _suffix;

    public override void LoadAttributes(JObject json, Dictionary<string, float> initialVars)
    {
        Debug.Log("[ChaoticModifier] Loading attributes");

        // Load custom name if provided
        _suffix = json["name"]?.Value<string>() ?? _suffix;

        // Load multiplier expression
        if (json["damage_multiplier"] != null)
        {
            string expr = json["damage_multiplier"].Value<string>();
            _damageMultiplier = RPNEvaluator.SafeEvaluateFloat(expr, initialVars, _damageMultiplier);
            Debug.Log($"[ChaoticModifier] multiplier={_damageMultiplier}");
        }

        // Base modifier loading
        base.LoadAttributes(json, initialVars);
    }

    // Inject multiplication on damage
    protected override void InjectMods(StatBlock mods)
    {
        Debug.Log($"[ChaoticModifier] Injecting damage × {_damageMultiplier}");
        mods.DamageMods.Add(new ValueMod(ModOp.Mul, _damageMultiplier));
    }

    // Apply spiraling effect after base cast
    protected override IEnumerator ApplyModifierEffect(Vector3 origin, Vector3 target)
    {
        Debug.Log("[ChaoticModifier] Applying spiraling effect");

        // Handle specific inner spell types
        if (inner is ArcaneSpray)
            yield return CreateSpiralingSpray(origin, target);
        else if (inner is ArcaneBlast)
            yield return CreateSpiralingBlast(origin, target);
        else
            yield return CreateGenericSpiraling(origin, target);
    }

    // Spiraling spray implementation
    private IEnumerator CreateSpiralingSpray(Vector3 origin, Vector3 target)
    {
        float baseAngle = Mathf.Atan2((target - origin).y, (target - origin).x) * Mathf.Rad2Deg;
        int count = Mathf.RoundToInt(inner.Damage) + 5;
        float angleStep = 60f / (count - 1);
        float startAngle = baseAngle - 30f;

        for (int i = 0; i < count; i++)
        {
            float angleDeg = startAngle + i * angleStep;
            Vector3 dir = new Vector3(Mathf.Cos(angleDeg * Mathf.Deg2Rad), Mathf.Sin(angleDeg * Mathf.Deg2Rad), 0f);

            GameManager.Instance.projectileManager.CreateProjectile(
                inner.IconIndex,
                "spiraling",
                origin,
                dir,
                inner.Speed,
                (hit, _) =>
                {
                    if (hit.team != owner.team)
                    {
                        int amt = Mathf.RoundToInt(Damage);
                        hit.Damage(new global::Damage(amt, global::Damage.Type.ARCANE));
                    }
                },
                0.1f + inner.Speed / 40f);

            yield return new WaitForSeconds(0.02f);
        }
    }

    // Spiraling blast implementation
    private IEnumerator CreateSpiralingBlast(Vector3 origin, Vector3 target)
    {
        Vector3 dir = (target - origin).normalized;
        GameManager.Instance.projectileManager.CreateProjectile(
            inner.IconIndex,
            "spiraling",
            origin,
            dir,
            inner.Speed,
            (hit, impact) =>
            {
                if (hit.team != owner.team)
                {
                    int amt = Mathf.RoundToInt(Damage);
                    hit.Damage(new global::Damage(amt, global::Damage.Type.ARCANE));
                    CreateSpiralingSecondaryExplosion(impact, amt / 4);
                }
            });

        yield return null;
    }

    // Helper for secondary spiraling explosion
    private void CreateSpiralingSecondaryExplosion(Vector3 center, int damage)
    {
        int count = 8;
        float angleStep = 360f / count;

        for (int i = 0; i < count; i++)
        {
            float angleDeg = i * angleStep;
            Vector3 dir = new Vector3(Mathf.Cos(angleDeg * Mathf.Deg2Rad), Mathf.Sin(angleDeg * Mathf.Deg2Rad), 0f);

            GameManager.Instance.projectileManager.CreateProjectile(
                inner.IconIndex,
                "spiraling",
                center,
                dir,
                inner.Speed * 0.8f,
                (hit, _) =>
                {
                    if (hit.team != owner.team)
                        hit.Damage(new global::Damage(damage, global::Damage.Type.ARCANE));
                },
                0.3f);
        }
    }

    // Generic spiraling for other spells
    private IEnumerator CreateGenericSpiraling(Vector3 origin, Vector3 target)
    {
        GameManager.Instance.projectileManager.CreateProjectile(
            inner.IconIndex,
            "spiraling",
            origin,
            (target - origin).normalized,
            inner.Speed,
            (hit, _) =>
            {
                if (hit.team != owner.team)
                {
                    int amt = Mathf.RoundToInt(Damage);
                    hit.Damage(new global::Damage(amt, global::Damage.Type.ARCANE));
                }
            });

        yield return null;
    }
}
