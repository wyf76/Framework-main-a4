using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro;

public class ClassSelector : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Dropdown classDropdown;

    private Dictionary<string, CharacterClassDefinition> classDefs;

    void Start()
    {
        // Ensure this GameObject is active
        gameObject.SetActive(true);
        Debug.Log("[ClassSelector] GameObject activated at start");
        
        classDefs = GameDataLoader.Classes;

        // Clear the options that might be set in the editor
        classDropdown.ClearOptions();

        // Create a new list that can hold both text and images for the dropdown
        var options = new List<TMP_Dropdown.OptionData>();

        // Loop through each class definition loaded from your data
        foreach (var kvp in classDefs)
        {
            string className = kvp.Key;
            CharacterClassDefinition classDef = kvp.Value;

            // Get the sprite for the current class using the PlayerSpriteManager
            Sprite classSprite = GameManager.Instance.playerSpriteManager.Get(classDef.sprite);

            // Create a new dropdown option with the class name AND its sprite
            options.Add(new TMP_Dropdown.OptionData(className, classSprite, Color.white));
        }

        // Add the fully-configured options to the dropdown
        classDropdown.AddOptions(options);

        // Refresh the dropdown to show the new options
        classDropdown.RefreshShownValue(); 

        // Set the default class when the game starts
        if (classDefs.Count > 0)
        {
            var className = classDropdown.options[0].text;
            GameManager.Instance.SetSelectedClass(classDefs[className]);
            Debug.Log($"Default class selected: {className}");
        }
        
        // Listen for when the user changes the selection
        classDropdown.onValueChanged.AddListener(OnClassSelected);
    }

    void OnClassSelected(int index)
    {
        var className = classDropdown.options[index].text;
        var def = classDefs[className];
        
        GameManager.Instance.SetSelectedClass(def);
        Debug.Log($"Class selected: {className}");
    }
}