using UnityEngine;
using TMPro;

public class MenuSelectorController : MonoBehaviour
{
    public TextMeshProUGUI label;
    public string level;
    public EnemySpawnerController spawner;
    public GameObject mainMenuPanel; // This is the important one

    public void SetLevel(string text)
    {
        level = text;
        if (label != null) label.text = text;    
    }

    public void StartLevel()
    {
        Debug.Log("StartLevel button clicked! (MenuSelectorController)");

        if (mainMenuPanel != null)
        {
            Debug.Log("MainMenuPanel is assigned in MenuSelectorController. Hiding it now.");
            mainMenuPanel.SetActive(false);
        }
        else
        {
            Debug.LogWarning("CRITICAL ERROR: MainMenuPanel has NOT been assigned in the MenuSelectorController Inspector!");
        }

        // --- Start of fix ---
        // Find the ClassSelector and hide its panel.
        var classSelector = FindFirstObjectByType<ClassSelector>();
        if (classSelector != null)
        {
            classSelector.gameObject.SetActive(false);
        }
        // --- End of fix ---

        if (spawner != null)
        {
            Debug.Log("Spawner is assigned. Starting spawner for level: " + level);
            spawner.StartLevel(level);
        }
        else
        {
            Debug.LogWarning("CRITICAL ERROR: Spawner has NOT been assigned in the MenuSelectorController Inspector!");
        }
    }
}