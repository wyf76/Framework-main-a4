using UnityEngine;
using System;

public class EventBus 
{
    private static EventBus theInstance;
    public static EventBus Instance
    {
        get
        {
            if (theInstance == null)
                theInstance = new EventBus();
            return theInstance;
        }
    }

    public event Action<Vector3, Damage, Hittable> OnDamage;
    public event Action<Hittable> OnEnemyDeath;
    public event Action<float> OnPlayerMove;
    public event Action OnSpellCast;

    public void DoDamage(Vector3 where, Damage dmg, Hittable target)
    {
        OnDamage?.Invoke(where, dmg, target);
    }

    public void EnemyDied(Hittable target)
    {
        OnEnemyDeath?.Invoke(target);
    }

    public void PlayerMoved(float distance)
    {
        OnPlayerMove?.Invoke(distance);
    }

    public void SpellWasCast()
    {
        OnSpellCast?.Invoke();
    }

}
