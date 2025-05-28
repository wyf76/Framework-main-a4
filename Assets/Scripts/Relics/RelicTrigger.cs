using UnityEngine;

public abstract class RelicTrigger
{
    protected PlayerController player;
    protected Relic relic;

    public virtual void Register(PlayerController player, Relic relic)
    {
        this.player = player;
        this.relic = relic;
    }

    public virtual void Unregister() { }
}
