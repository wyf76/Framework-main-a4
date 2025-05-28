using UnityEngine;

public class DistanceMovedTrigger : RelicTrigger
{
    private float distance;
    private float accumulated;

    public DistanceMovedTrigger(float dist)
    {
        distance = dist;
    }

    public override void Register(PlayerController player, Relic relic)
    {
        base.Register(player, relic);
        EventBus.Instance.OnPlayerMove += OnMove;
    }

    void OnMove(float dist)
    {
        accumulated += dist;
        if (accumulated >= distance)
        {
            accumulated = 0f;
            relic.Activate(player);
        }
    }

    public override void Unregister()
    {
        EventBus.Instance.OnPlayerMove -= OnMove;
    }
}
