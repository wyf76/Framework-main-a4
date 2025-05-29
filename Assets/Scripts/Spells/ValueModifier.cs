using System.Collections.Generic;

public enum ModType { Additive, Multiplicative }
public class ValueModifier
{
    public ModType type; public float value;
    public ValueModifier(ModType t,float v){type=t;value=v;}

    public static float Apply(List<ValueModifier> mods,float baseVal)
    {
        float result=baseVal; float add=0;
        foreach(var m in mods)
            if(m.type==ModType.Multiplicative) result*=m.value;
            else add+=m.value;
        return result+add;
    }
}