using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpellCaster : MonoBehaviour
{
    [Header("Mana Settings")]
    public int max_mana;
    public int mana;
    public int mana_reg;
    public int nextSpellManaDiscount = 0; // New field for relic effect
    public static event System.Action OnSpellCast;

    [Header("Team")]
    public Hittable.Team team;

    [Header("Spells")]
    public int spellPower;
    public List<Spell> spells = new(4);

    void Awake()
    {
        StartCoroutine(ManaRegeneration());
        var builder = new SpellBuilder();
        spells.Add(builder.Build(this));
        while (spells.Count < 4)
            spells.Add(null);
    }

    IEnumerator ManaRegeneration()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            mana = Mathf.Min(max_mana, mana + mana_reg);
        }
    }

    public IEnumerator CastSlot(int slot, Vector3 from, Vector3 to)
    {
        if (slot < 0 || slot >= spells.Count) yield break;

        Spell s = spells[slot];
        if (s == null)
        {
            Debug.LogWarning($"[SpellCaster] No spell in slot {slot}");
            yield break;
        }
        
        // Calculate the actual mana cost with the discount
        int finalManaCost = Mathf.Max(0, Mathf.RoundToInt(s.Mana) - nextSpellManaDiscount);

        if (!s.IsReady || mana < finalManaCost)
        {
            yield break;
        }
        
        Debug.Log($"[SpellCaster] Slot {slot} -> Casting \"{s.DisplayName}\" (mana={mana}, cost={finalManaCost})");
        
        mana -= finalManaCost;
        s.lastCast = Time.time;
        nextSpellManaDiscount = 0; 

        OnSpellCast?.Invoke(); 

        
        yield return s.TryCast(from, to);
    }
}