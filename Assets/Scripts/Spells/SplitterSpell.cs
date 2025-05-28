using System.Collections;
using UnityEngine;

public class SplitterSpell : ModifierSpell
{
    private float angle;

    public SplitterSpell(SpellCaster owner, Spell inner, float angle)
        : base(owner, inner)
    {
        this.angle = angle;
    }

    public override IEnumerator Cast(Vector3 where, Vector3 target, Hittable.Team team)
    {
        Vector3 dir = (target - where).normalized;

        // Rotate 
        Vector3 dir1 = Quaternion.Euler(0, 0, angle) * dir;
        Vector3 dir2 = Quaternion.Euler(0, 0, -angle) * dir;

        Vector3 target1 = where + dir1 * 5f;
        Vector3 target2 = where + dir2 * 5f;

        yield return owner.Cast(where, target1);  
        yield return owner.Cast(where, target2);  
    }
}