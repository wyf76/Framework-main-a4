using UnityEngine;
using Newtonsoft.Json;
using System.Collections.Generic;

public static class GameDataLoader
{
    private static Dictionary<string, CharacterClassDefinition> _classes;

    /// <summary>Mapping from class name to its definition (sprite + formulas)</summary>
    public static Dictionary<string, CharacterClassDefinition> Classes
    {
        get
        {
            if (_classes == null)
                LoadClasses();
            return _classes;
        }
    }

    private static void LoadClasses()
    {
        var text = Resources.Load<TextAsset>("classes"); // classes.json in Resources/
        if (text == null)
        {
            Debug.LogError("GameDataLoader: could not find classes.json in Resources/");
            _classes = new Dictionary<string, CharacterClassDefinition>();
            return;
        }
        _classes = JsonConvert.DeserializeObject<Dictionary<string, CharacterClassDefinition>>(text.text);
        Debug.Log($"GameDataLoader: Loaded {_classes.Count} character classes.");
    }
    
    static List<RelicDefinition> relicDefs;
    public static List<RelicDefinition> Relics
    {
        get
        {
            if (relicDefs == null)
                LoadRelics();
            return relicDefs;
        }
    }

    private static void LoadRelics()
    {
        var txt = Resources.Load<TextAsset>("relics");      // relics.json in Resources/
        if (txt == null)
        {
            Debug.LogError("GameDataLoader: cannot find relics.json");
            relicDefs = new List<RelicDefinition>();
            return;
        }
        relicDefs = JsonConvert.DeserializeObject<List<RelicDefinition>>(txt.text);
        Debug.Log($"GameDataLoader: loaded {relicDefs.Count} relics");
    }
}