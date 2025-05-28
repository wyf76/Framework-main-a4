using UnityEngine;

public class DealDamageTrigger : RelicTrigger
{
    public override void Register(PlayerController player, Relic relic)
    {
        base.Register(player, relic);
        EventBus.Instance.OnDamage += OnDamage;
    }

    void OnDamage(Vector3 pos, Damage dmg, Hittable target)
    {
        if (target.team == Hittable.Team.MONSTERS)
        {
            relic.Activate(player);
        }
    }

    public override void Unregister()
    {
        EventBus.Instance.OnDamage -= OnDamage;
    }
}
