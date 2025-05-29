using System;

public static class RelicEventBus
{
    // This event is triggered when a projectile expires without hitting anything.
    public static event Action OnSpellMissed;

    public static void SpellMissed()
    {
        OnSpellMissed?.Invoke();
    }

    // This event is triggered when the player moves a certain distance.
    public static event Action<float> OnPlayerMovedDistance;

    public static void PlayerMovedDistance(float distance)
    {
        OnPlayerMovedDistance?.Invoke(distance);
    }
}