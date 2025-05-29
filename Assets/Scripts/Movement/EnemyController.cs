using UnityEngine;
using System.Collections;

// Controls enemy behavior: movement toward player, attacks, health, and slow effects.

public class EnemyController : MonoBehaviour
{
    [Header("Combat")]
    public int damage;
    public float attackCooldown = 2f;

    [Header("Movement")]
    public int speed;
    private float currentSpeed;

    [Header("Health")]
    public Hittable hp;
    public HealthBar healthui;

    private Transform playerTransform;
    private float lastAttackTime;
    private bool dead;

    void Start()
    {
        playerTransform = GameManager.Instance.player.transform;
        hp.OnDeath += Die;
        healthui.SetHealth(hp);
    }

    void Update()
    {
        Vector3 direction = playerTransform.position - transform.position;
        if (direction.magnitude < 2f)
        {
            DoAttack();
        }
        else
        {
            GetComponent<Unit>().movement = direction.normalized * speed;
        }
    }
    
    void DoAttack()
    {
        if (lastAttackTime + 2 < Time.time)
        {
            lastAttackTime = Time.time;
            playerTransform.gameObject.GetComponent<PlayerController>().hp.Damage(new Damage(5, Damage.Type.PHYSICAL));
        }
    }


    // <param name="duration">Duration of slow in seconds.</param>
    // <param name="slowFactor">Multiplier to speed (0 to 1).</param>
    public void ApplySlow(float duration, float slowFactor)
    {
        // Prevent stacking slows
        StopAllCoroutines();
        StartCoroutine(SlowRoutine(duration, slowFactor));
    }

    private IEnumerator SlowRoutine(float duration, float slowFactor)
    {
        currentSpeed = speed * slowFactor;
        yield return new WaitForSeconds(duration);
        currentSpeed = speed;
    }

    void Die()
    {
        if (!dead)
        {
            dead = true;
            GameManager.Instance.RemoveEnemy(gameObject);
            Destroy(gameObject);
        }
    }
}