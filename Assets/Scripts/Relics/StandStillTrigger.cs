using System.Collections;
using UnityEngine;

public class StandStillTrigger : RelicTrigger
{
    private float wait;
    private float lastMove;
    private bool active;

    public StandStillTrigger(float wait)
    {
        this.wait = wait;
    }

    public override void Register(PlayerController player, Relic relic)
    {
        base.Register(player, relic);
        lastMove = Time.time;
        EventBus.Instance.OnPlayerMove += OnMove;
        CoroutineManager.Instance.Run(Check());
    }

    void OnMove(float dist)
    {
        lastMove = Time.time;
        active = false;
    }

    IEnumerator Check()
    {
        while (true)
        {
            if (!active && Time.time - lastMove >= wait)
            {
                active = true;
                relic.Activate(player);
            }
            yield return null;
        }
    }

    public override void Unregister()
    {
        EventBus.Instance.OnPlayerMove -= OnMove;
    }
}
