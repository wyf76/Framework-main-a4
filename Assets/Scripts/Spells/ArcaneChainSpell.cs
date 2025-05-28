using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class ArcaneChainSpell : BaseSpell
{
    public ArcaneChainSpell(SpellCaster owner) : base(owner) { }

    public override IEnumerator Cast(Vector3 where, Vector3 target, Hittable.Team team)
    {
        this.team = team;
        GameManager.Instance.projectileManager.CreateProjectile(
            projectileSprite,
            GetTrajectory(),
            where,
            target - where,
            GetSpeed(),
            (hit, pos) => OnHit(hit, pos, 2)  // chain up to 2 more times
        );
        yield return new WaitForEndOfFrame();
    }

    void OnHit(Hittable current, Vector3 impact, int remainingChains)
    {
        if (current.team != team)
        {
            current.Damage(new Damage(GetDamage(), Damage.Type.ARCANE));

            if (remainingChains > 0)
            {
                GameObject currentGO = current.GetGameObject();
                GameObject next = GameManager.Instance.GetClosestEnemy(currentGO.transform.position);

                if (next != null && next != currentGO)
                {
                    Vector3 from = currentGO.transform.position;
                    Vector3 to = next.transform.position;

                    GameManager.Instance.projectileManager.CreateProjectile(
                        projectileSprite,
                        "straight",
                        from,
                        to - from,
                        GetSpeed(),
                        (hit, pos) => OnHit(hit, pos, remainingChains - 1)
                    );
                }
            }
        }
    }
}