using UnityEngine;

public class Relic
{
    public string name;
    public int sprite;
    public RelicTrigger trigger;
    public RelicEffect effect;

    public Relic(string name, int sprite, RelicTrigger trigger, RelicEffect effect)
    {
        this.name = name;
        this.sprite = sprite;
        this.trigger = trigger;
        this.effect = effect;
    }

    public void Register(PlayerController player)
    {
        trigger.Register(player, this);
    }

    public void Activate(PlayerController player)
    {
        effect.Apply(player);
    }

    public void Deactivate(PlayerController player)
    {
        effect.Remove(player);
    }
}
