using UnityEngine;
using System;

public class Hittable
{
    public enum Team { PLAYER, MONSTERS }

    [Header("Health Settings")]

    public Team team;

    public int hp;
    public int max_hp;

    public GameObject owner;

    public event Action OnDeath;
    public event Action<int,int> OnHealthChanged;

    public Hittable(int hp, Team team, GameObject owner)
    {
        this.hp     = hp;
        this.max_hp = hp;
        this.team   = team;
        this.owner  = owner;
    }

    public void Damage(Damage damage)
    {
        EventBus.Instance.DoDamage(owner.transform.position, damage, this);
        hp -= damage.amount;
        if (hp <= 0)
        {
            hp = 0;
            OnDeath?.Invoke();
        }
    }
    public void Heal(int amount)
    {
        hp += amount;
        hp = Mathf.Min(hp, max_hp);
        OnHealthChanged?.Invoke(hp, max_hp);
    }
    public void SetMaxHP(int newMaxHP, bool preservePercentage = true)
    {
        if (preservePercentage)
        {
            float perc = (float)hp / max_hp;
            max_hp = newMaxHP;
            hp     = Mathf.RoundToInt(perc * max_hp);
        }
        else
        {
            max_hp = newMaxHP;
            hp     = newMaxHP;
        }
        OnHealthChanged?.Invoke(hp, max_hp);
    }
}
