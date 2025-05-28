using UnityEngine;

public class KillTrigger : RelicTrigger
{
    public override void Register(PlayerController player, Relic relic)
    {
        base.Register(player, relic);
        EventBus.Instance.OnEnemyDeath += OnEnemyDeath;
    }

    void OnEnemyDeath(Hittable enemy)
    {
        relic.Activate(player);
    }

    public override void Unregister()
    {
        EventBus.Instance.OnEnemyDeath -= OnEnemyDeath;
    }
}
