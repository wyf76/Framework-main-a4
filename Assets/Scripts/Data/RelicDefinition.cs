using System;
using UnityEngine;

[Serializable]
public class TriggerDefinition
{
    // This is what the player sees, explaining when the relic might go off.

    public string description;

    // This is our internal keyword for the type of trigger, like "take-damage" or "stand-still".
    public string type;

    // Some triggers might need a specific value, like how long to wait for an "stand-still" trigger.
    public string amount;
}

[Serializable]
public class EffectDefinition
{
    // This tells the player what cool thing happens when the relic activates.
    public string description;

    // Our internal keyword for the effect
    public string type;

    // This defines how strong the effect is.
    public string amount;

    // This is for effects that last until a certain condition is met.
    public string until;

    // This defines a cooldown for the effect. (This is the new field)
    public string cooldown; 
}

[Serializable]
public class RelicDefinition
{
    // The name of the relic as it appears in the game, e.g., "Green Gem".
    public string name;

    // This is just a number that tells the game which picture to show for this relic.
    public int sprite;

    // Holds all the details about what makes this relic trigger.
    public TriggerDefinition trigger;

    // And this holds all the details about what happens when it does trigger.
    public EffectDefinition effect;
}