using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;


// Chooses a base spell and random modifiers based on the current wave.

public class SpellBuilder
{
    private readonly Dictionary<string, JObject> catalog;
    private readonly System.Random rng = new System.Random();

    static readonly string[] BaseKeys     = {"arcane_bolt","arcane_spray","magic_missile","arcane_blast", "knockback_bolt"};
    static readonly string[] ModifierKeys = {"splitter","doubler","damage_amp","speed_amp","chaos","homing","frost_spike_modifier","vampiric_essence_modifier"};

    public SpellBuilder()
    {
        var ta = Resources.Load<TextAsset>("spells");
        catalog = ta != null
            ? JsonConvert.DeserializeObject<Dictionary<string,JObject>>(ta.text)
            : new Dictionary<string,JObject>();
    }

    public Spell Build(SpellCaster owner)
    {
        int wave = GetCurrentWave();
        var vars = new Dictionary<string,float> { ["power"]=owner.spellPower, ["wave"]=wave };

        if (wave <= 1)
            return BuildBase(owner, "arcane_bolt", vars);

        // Pick random base
        int bidx = rng.Next(BaseKeys.Length);
        Spell s = BuildBase(owner, BaseKeys[bidx], vars);

        // Random number of modifiers
        int modCount = rng.NextDouble() < 0.3 ? 2 : rng.Next(2);
        for (int i = 0; i < modCount; i++)
            s = ApplyModifier(s, ModifierKeys[rng.Next(ModifierKeys.Length)], vars);

        return s;
    }

    public Spell BuildBase(SpellCaster owner, string key, Dictionary<string,float> vars)
    {
        Spell s = key switch
        {
            "arcane_bolt"     => new ArcaneBolt(owner),
            "arcane_spray"    => new ArcaneSpray(owner),
            "magic_missile"   => new MagicMissile(owner),
            "arcane_blast"    => new ArcaneBlast(owner),
            "knockback_bolt"  => new KnockbackSpell(owner),
            "heal"            => new HealSpell(owner),
            "speed_buff"      => new SpeedBuffSpell(owner),
            _                 => new ArcaneBolt(owner)
        };

        if (catalog.TryGetValue(key, out var json))
            s.LoadAttributes(json, vars);

        return s;
    }


    public Spell ApplyModifier(Spell inner, string mkey, Dictionary<string,float> vars)
    {
        Spell mod = mkey switch
        {
            "splitter"                 => new Splitter(inner),
            "doubler"                  => new Doubler(inner),
            "damage_amp"               => new DamageMagnifier(inner),
            "speed_amp"                => new SpeedModifier(inner),
            "chaos"                    => new ChaoticModifier(inner),
            "homing"                   => new HomingModifier(inner),
            "vampiric_essence_modifier"=> new VampiricEssenceModifier(inner),
            "frost_spike_modifier"     => new FrostSpikeModifier(inner),
            _                          => inner
        };

        if (catalog.TryGetValue(mkey, out var json))
            mod.LoadAttributes(json, vars);

        return mod;
    }
    private int GetCurrentWave()
    {
        var sp = UnityEngine.Object.FindFirstObjectByType<EnemySpawnerController>();
        return sp != null ? sp.currentWave : 1;
    }
}
