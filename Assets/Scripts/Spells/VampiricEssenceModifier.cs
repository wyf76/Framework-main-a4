using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

public sealed class VampiricEssenceModifier : ModifierSpell
{
    private float  _lifeStealPercent = 0.2f;
    private string _modifierName    = "vampiric";        // default suffix

    public VampiricEssenceModifier(Spell inner) : base(inner) { }

    protected override string Suffix => _modifierName;   // what ShowReward prints

    public override void LoadAttributes(JObject json, Dictionary<string, float> vars)
    {
        // 1) pull the pretty display name from JSON
        _modifierName = json["name"]?.Value<string>() ?? _modifierName;

        // 2) parse your lifesteal fields
        var eff = json["effects"]![0];
        if (eff["type"]!.Value<string>() == "lifesteal")
            _lifeStealPercent = float.Parse(eff["percent"]!.Value<string>());

        base.LoadAttributes(json, vars);
    }

    // You still need a no‐op InjectMods override
    protected override void InjectMods(StatBlock mods) { }

    protected override IEnumerator ApplyModifierEffect(Vector3 origin, Vector3 target)
    {
        // Capture existing projectiles
        var before = Object.FindObjectsByType<ProjectileController>(FindObjectsSortMode.None);

        // Cast the inner spell
        yield return inner.TryCast(origin, target);

        // Hook only the newly spawned projectiles
        var after = Object.FindObjectsByType<ProjectileController>(FindObjectsSortMode.None);
        foreach (var proj in after)
        {
            if (System.Array.IndexOf(before, proj) < 0)
            {
                proj.OnHit += (hitReceiver, impactPos) =>
                {
                    if (hitReceiver.team != owner.team)
                    {
                        // Compute heal amount from this spell's damage
                        int healAmt = Mathf.RoundToInt(inner.Damage * _lifeStealPercent);

                        // Find the caster's Hittable and heal it
                        var casterHittable = owner.GetComponent<Hittable>();
                        if (casterHittable != null)
                            casterHittable.Heal(healAmt);
                    }
                };
            }
        }
    }
}
