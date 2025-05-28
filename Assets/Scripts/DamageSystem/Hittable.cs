using UnityEngine;
using System;

public class Hittable
{

    public enum Team { PLAYER, MONSTERS }
    public Team team;

    public int hp;
    public int max_hp;

    public GameObject owner;

    public void Damage(Damage damage)
    {
        EventBus.Instance.DoDamage(owner.transform.position, damage, this);
        hp -= damage.amount;
        if (this.team == Team.PLAYER && GameManager.Instance.state != GameManager.GameState.GAMEOVER)
        {
            GameManager.Instance.damageReceived += damage.amount;
        }
        else if (this.team == Team.MONSTERS && GameManager.Instance.state != GameManager.GameState.GAMEOVER)
        {
            GameManager.Instance.damageDealt += damage.amount;
        }
        if (hp <= 0)
        {
            hp = 0;
            OnDeath();
        }
    }

    public event Action OnDeath;

    public Hittable(int hp, Team team, GameObject owner)
    {
        this.hp = hp;
        this.max_hp = hp;
        this.team = team;
        this.owner = owner;
    }

    public void SetMaxHP(int max_hp)
    {
        float perc = this.hp * 1.0f / this.max_hp;
        this.max_hp = max_hp;
        this.hp = Mathf.RoundToInt(perc * max_hp);
    }
    public GameObject GetGameObject()
    {
        return owner;
    }
}
