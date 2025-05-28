using System.Collections;
using UnityEngine;

public class CheapCastSpell : ModifierSpell
{
    public CheapCastSpell(SpellCaster owner, Spell inner) : base(owner, inner) {}

    public override int GetManaCost()
    {
        return (int)(inner.GetManaCost() * 0.5f);
    }
}