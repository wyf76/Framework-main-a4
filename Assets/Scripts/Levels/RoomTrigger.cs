using UnityEngine;

public class RoomTrigger : MonoBehaviour
{
    public string roomID;
    public string spawnLocationTag = "random"; // Match what's used in level data

    private bool triggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;

        if (other.CompareTag("Player"))
        {
            Debug.Log("Entered Room: " + roomID);
            triggered = true;

            EnemySpawnerController spawner = FindObjectOfType<EnemySpawnerController>();
            if (spawner != null)
            {
                spawner.SpawnEnemiesInRoom(spawnLocationTag);
            }

            // Optional: disable trigger after use
            gameObject.SetActive(false);
        }
    }
}