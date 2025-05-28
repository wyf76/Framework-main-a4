using UnityEngine;

public abstract class RelicEffect
{
    public abstract void Apply(PlayerController player);
    public virtual void Remove(PlayerController player) { }
}
