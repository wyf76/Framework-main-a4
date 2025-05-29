using UnityEngine;

public class RelicUIManager : MonoBehaviour
{
    public GameObject relicUIPrefab;
    public PlayerController player;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log("RelicUIManager: Initialized and ready");
    }

    // Update is called once per frame
    void Update()
    {

    }

    // Add a relic through this manager (calls RelicUI.Instance)

    public void AddRelic(Relic relic)
    {
        if (RelicUI.Instance != null)
        {
            RelicUI.Instance.AddRelic(relic);
            Debug.Log($"RelicUIManager: Forwarded relic '{relic.Name}' to RelicUI");
        }
        else
        {
            Debug.LogError("RelicUIManager: RelicUI.Instance is null! Make sure RelicUI component exists in scene.");
        }
    }

    // Remove a relic through this manager

    public void RemoveRelic(string relicName)
    {
        if (RelicUI.Instance != null)
        {
            RelicUI.Instance.RemoveRelic(relicName);
        }
    }

    // Clear all relics through this manager

    public void ClearAllRelics()
    {
        if (RelicUI.Instance != null)
        {
            RelicUI.Instance.ClearAllRelics();
        }
    }

    [ContextMenu("Test Add Relic")]
    public void TestAddRelic()
    {
        var testRelicData = new RelicData
        {
            name = "Debug Relic",
            sprite = 0,
            trigger = new TriggerData { type = "test" },
            effect = new EffectData { type = "test" }
        };

        var testRelic = new Relic(testRelicData);
        AddRelic(testRelic);

        Debug.Log("RelicUIManager: Added test relic");
    }

}