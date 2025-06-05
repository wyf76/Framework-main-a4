using System.Collections.Generic;
using UnityEngine;

// Holds deserialized list of enemies
[System.Serializable]
public class EnemyList
{
    public List<Enemy> enemies;
}

// Holds deserialized list of levels
[System.Serializable]
public class LevelList
{
    public List<Level> levels;
}

[System.Serializable]
public class Enemy
{
    public string name;    // unique identifier
    public int sprite;     // sprite index
    public int hp;         // base health
    public float speed;    // base move speed
    public int damage;     // base damage
    public string spell;   // optional spell key
    public float spell_range;
}

[System.Serializable]
public class Level
{
    public string name;       // display name
    public int waves;         // number of waves, <=0 means endless
    public List<Spawn> spawns; // spawn definitions per wave
}

[System.Serializable]
public class Spawn
{
    public string enemy;          // matches Enemy.name
    public string count;          // RPN for how many to spawn
    public List<int> sequence;    // optional spawn pattern
    public string delay;          // RPN or numeric delay between batches
    public string location;       
    public string hp;             // RPN or "base" for health override
    public string speed;          // RPN or "base" for speed override
    public string damage;         // RPN or "base" for damage override
}

