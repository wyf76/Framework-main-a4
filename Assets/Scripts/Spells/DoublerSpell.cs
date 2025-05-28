using System.Collections;
using UnityEngine;

public class DoublerSpell : ModifierSpell
{
    private float delay;

    public DoublerSpell(SpellCaster owner, Spell inner, float delay)
        : base(owner, inner)
    {
        this.delay = delay;
    }

    public override IEnumerator Cast(Vector3 where, Vector3 target, Hittable.Team team)
    {
        // 1st cast
        Debug.Log("DoublerSpell: Casting first instance");
        yield return inner.Cast(where, target, team);

        // Again
        yield return new WaitForSeconds(delay);
        Debug.Log("DoublerSpell: Casting second instance after delay");
        yield return inner.Cast(where, target, team);
    }
}