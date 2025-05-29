using System.Collections.Generic;

//Holds additive and multiplicative stat modifiers for spells.

public class StatBlock
{
    public List<ValueMod> DamageMods { get; } = new();
    public List<ValueMod> ManaMods   { get; } = new();
    public List<ValueMod> SpeedMods  { get; } = new();
    public List<ValueMod> CooldownMods { get; } = new();

    //Applies a sequence of modifiers to a base value.
    public static float Apply(float baseValue, List<ValueMod> mods)
    {
        foreach (var mod in mods)
        {
            baseValue = mod.Op == ModOp.Add
                ? baseValue + mod.Value
                : baseValue * mod.Value;
        }
        return baseValue;
    }
}