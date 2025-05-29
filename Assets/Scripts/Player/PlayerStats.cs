using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public int MaxHealth { get; private set; }
    public int MaxMana { get; private set; }
    public int ManaRegen { get; private set; }
    public int SpellPower { get; private set; }
    public int Speed { get; private set; }

    public void SetStats(int maxHealth, int maxMana, int manaRegen, int spellPower, int speed)
    {
        MaxHealth = maxHealth;
        MaxMana = maxMana;
        ManaRegen = manaRegen;
        SpellPower = spellPower;
        Speed = speed;

        // TODO: Reset current health/mana and update UI as needed
    }
}