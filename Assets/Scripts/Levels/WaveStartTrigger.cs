using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class WaveStartTrigger : MonoBehaviour
{
    private bool used = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (used) return;

        // Check if it's the player
        if (other.CompareTag("unit"))
        {
            Debug.Log("Player entered wave start trigger.");

            // Check if wave has ended and all enemies are dead
            bool waveEnded = GameManager.Instance.state == GameManager.GameState.WAVEEND;
            bool allEnemiesDead = GameManager.Instance.enemy_count <= 0;

            Debug.Log($"Wave ended: {waveEnded}, Enemies dead: {allEnemiesDead}");

            if (waveEnded && allEnemiesDead)
            {
                Debug.Log("Conditions met — showing reward screen.");
                RewardScreenManager.Instance.ShowRewardScreenPublic();

                used = true;
                Destroy(gameObject); // Optional: remove the trigger so it can't be used again
            }
            else
            {
                Debug.Log("Cannot show reward screen — conditions not met.");
            }
        }
    }
}