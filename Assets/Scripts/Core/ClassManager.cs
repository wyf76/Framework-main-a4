using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;


public class ClassManager : MonoBehaviour
{
    public static Dictionary<string, PlayerClass> Classes;

    private void Awake()
    {
        Debug.Log("ClassManager Awake called");
        TextAsset json = Resources.Load<TextAsset>("classes");
        Classes = JsonConvert.DeserializeObject<Dictionary<string, PlayerClass>>(json.text);
    }
}

public class PlayerClass
{
    public int sprite;
    public string health;
    public string mana;
    public string mana_regeneration;
    public string spellpower;
    public string speed;

}
