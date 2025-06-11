using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIDebugger : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(DebugUIState());
    }

    IEnumerator DebugUIState()
    {
        // Wait a frame to ensure all Start methods have run
        yield return null;
        
        Debug.Log("=== UI DEBUGGER - Checking UI State ===");
        Debug.Log("Game State: " + GameManager.Instance.state);
        
        // Find important UI elements
        var spawner = FindFirstObjectByType<EnemySpawnerController>();
        if (spawner != null)
        {
            Debug.Log("EnemySpawnerController found");
            Debug.Log("- mainMenuPanel: " + (spawner.mainMenuPanel != null ? "Assigned, Active: " + spawner.mainMenuPanel.activeSelf : "NULL"));
            Debug.Log("- level_selector: " + (spawner.level_selector != null ? "Assigned, Active: " + spawner.level_selector.gameObject.activeSelf : "NULL"));
        }
        else
        {
            Debug.LogError("EnemySpawnerController NOT FOUND!");
        }
        
        var classSelector = FindFirstObjectByType<ClassSelector>();
        if (classSelector != null)
        {
            Debug.Log("ClassSelector found - Active: " + classSelector.gameObject.activeSelf);
        }
        else
        {
            Debug.LogError("ClassSelector NOT FOUND!");
        }
        
        // Check for Canvas
        var canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        Debug.Log($"Found {canvases.Length} Canvas(es):");
        foreach (var canvas in canvases)
        {
            Debug.Log($"- Canvas '{canvas.name}' - Active: {canvas.gameObject.activeSelf}, Enabled: {canvas.enabled}");
        }
        
        // Check for any Button components
        var buttons = FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        Debug.Log($"Found {buttons.Length} Button(s) (including inactive):");
        int activeButtons = 0;
        foreach (var button in buttons)
        {
            if (button.gameObject.activeInHierarchy)
            {
                activeButtons++;
                Debug.Log($"- Active Button: '{button.name}'");
            }
        }
        Debug.Log($"Active buttons: {activeButtons}/{buttons.Length}");
        
        Debug.Log("=== END UI DEBUGGER ===");
    }
} 