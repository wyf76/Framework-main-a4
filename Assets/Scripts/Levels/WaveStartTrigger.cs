using UnityEngine;

public class WaveStartTrigger : MonoBehaviour
{
    private bool playerInside = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!playerInside && collision.CompareTag("unit")) // Or "Unit" if needed
        {
            playerInside = true;

            if (GameManager.Instance.enemy_count <= 0 &&
                GameManager.Instance.state == GameManager.GameState.WAVEEND)
            {
                Debug.Log("Player entered star — showing reward screen");
                RewardScreenManager.Instance.ShowRewardScreenPublic();

                Destroy(gameObject); // Remove star after use
            }
            else
            {
                Debug.Log("Enemies still alive — reward screen not shown");
            }
        }
    }
}