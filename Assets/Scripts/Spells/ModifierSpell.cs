using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModifierSpell : Spell
{
    protected Spell inner;
    private float damageMultiplier = 1f;
    private float manaMultiplier = 1f;
    private float cooldownMultiplier = 1f;
    private float speedMultiplier = 1f;
    private float manaAdder = 0f;
    private string overrideTrajectory = null;

    public ModifierSpell(SpellCaster owner, Spell innerSpell) : base(owner)
    {
        this.inner = innerSpell;
    }

    public void SetDamageMultiplier(float val) => damageMultiplier = val;
    public void SetManaMultiplier(float val) => manaMultiplier = val;
    public void SetCooldownMultiplier(float val) => cooldownMultiplier = val;
    public void SetSpeedMultiplier(float val) => speedMultiplier = val;
    public void SetManaAdder(float val) => manaAdder = val;
    public void SetOverrideTrajectory(string t) => overrideTrajectory = t;

    public override string GetName() => inner.GetName();
    public override int GetManaCost() => (int)(inner.GetManaCost() * manaMultiplier + manaAdder);
    public override int GetDamage() => (int)(inner.GetDamage() * damageMultiplier);
    public override float GetCooldown() => inner.GetCooldown() * cooldownMultiplier;
    public override int GetIcon() => inner.GetIcon();
    public override float GetSpeed()
    {
        return inner.GetSpeed() * speedMultiplier;
    }
    public string GetTrajectory()
    {
        return overrideTrajectory ?? (inner is BaseSpell bs ? bs.GetTrajectory() : "straight");
    }

    public override IEnumerator Cast(Vector3 where, Vector3 target, Hittable.Team team)
    {
        return inner.Cast(where, target, team);
    }
}