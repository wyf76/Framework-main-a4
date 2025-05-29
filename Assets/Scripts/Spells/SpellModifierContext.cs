using System.Collections.Generic;

public class SpellModifierContext
{
    public List<ValueModifier> damageMods=new();
    public List<ValueModifier> speedMods=new();
    public List<ValueModifier> manaMods=new();
    public string overrideTrajectory;
    public int overrideSprite;

    public float ApplyDamage(int v) => ValueModifier.Apply(damageMods,v);
    public float ApplySpeed(float v) => ValueModifier.Apply(speedMods,v);
    public float ApplyMana(int v)  => ValueModifier.Apply(manaMods,v);
    public float ApplyLifetime(float v)=>v; // unused
}