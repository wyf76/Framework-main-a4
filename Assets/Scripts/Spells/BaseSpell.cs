using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class BaseSpell : Spell
{
    public string name;
    public string description;
    public int icon;

    public string damageExpression;
    public string manaCostExpression;
    public string cooldownExpression;
    public string projectileSpeedExpression;

    public string trajectory;
    public int projectileSprite;

    public BaseSpell(SpellCaster owner) : base(owner) {}

    public override string GetName() => name;

    public override int GetManaCost() =>
        (int)RPNEvaluator.Evaluate(manaCostExpression, BuildVariables());

    public override int GetDamage() =>
        (int)RPNEvaluator.Evaluate(damageExpression, BuildVariables());

    public override float GetCooldown() =>
        float.Parse(cooldownExpression);

    public override int GetIcon() => icon;

    public override IEnumerator Cast(Vector3 where, Vector3 target, Hittable.Team team)
    {
        this.team = team;

        float speed = GetSpeed();

        GameManager.Instance.projectileManager.CreateProjectile(
            projectileSprite, GetTrajectory(), where, target - where, speed, OnHit);

        yield return new WaitForEndOfFrame();
    }

    void OnHit(Hittable other, Vector3 impact)
    {
        if (other.team != team)
        {
            other.Damage(new Damage(GetDamage(), Damage.Type.ARCANE));
        }
    }

    private Dictionary<string, float> BuildVariables()
    {
        return new Dictionary<string, float>
        {
            { "power", owner.spellPower },
            { "wave", GameManager.Instance.currentWave }
        };
    }

    public string GetTrajectory() => trajectory;

    public override float GetSpeed() =>
        RPNEvaluator.Evaluate(projectileSpeedExpression, BuildVariables());
}