using UnityEngine;
using System.Collections;
using System.Collections.Generic;


// Controls enemy behavior: movement toward player, attacks, health, and slow effects.
public class EnemyController : MonoBehaviour
{

    [Header("Combat")]
    public int damage;
    public float attackCooldown = 2f;

    public Collider2D roomBounds;

    public SpellCaster spellcaster;
    public string spellKey;
    public float spellRange = 6f;

    [Header("Movement")]
    public int speed;
    protected float currentSpeed;
    private float speedModifier = 1f;
    private float damageModifier = 1f;
    public float repathTime = 1f;

    [Header("Health")]
    public Hittable hp;
    public HealthBar healthui;

    protected Transform playerTransform;
    public string enemyType;

    private Queue<Vector3> path = new Queue<Vector3>();
    private float nextPath;
    private float lastAttackTime;
    private bool dead;

    protected virtual void Start()
    {
        playerTransform = GameManager.Instance.player.transform;
        hp.OnDeath += Die;
        healthui.SetHealth(hp);
        currentSpeed = speed;

        if (!string.IsNullOrEmpty(spellKey))
        {
            spellcaster = GetComponent<SpellCaster>();
            if (spellcaster == null)
                spellcaster = gameObject.AddComponent<SpellCaster>();
            spellcaster.team = Hittable.Team.MONSTERS;

            var builder = new SpellBuilder();
            var vars = new Dictionary<string, float> { { "power", spellcaster.spellPower }, { "wave", 1f } };
            spellcaster.spells[0] = builder.BuildBase(spellcaster, spellKey, vars);
            for (int i = 1; i < spellcaster.spells.Count; i++)
                spellcaster.spells[i] = null;
        }

        if (enemyType == "zombie")
            currentSpeed *= 1.5f;

        if (roomBounds == null)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 0.1f);
            foreach (var hit in hits)
            {
                if (hit.CompareTag("RoomBounds"))
                {
                    roomBounds = hit;
                    break;
                }
            }
        }
    }

    protected virtual void Update()
    {

        if (roomBounds != null && !roomBounds.bounds.Contains(playerTransform.position))
        {
            GetComponent<Unit>().movement = Vector2.zero;
            return; // Skip the rest of the Update
        }

        Vector3 direction = GetMoveDirection();

        if (!string.IsNullOrEmpty(spellKey) && spellcaster != null && direction.magnitude <= spellRange)
        {
            StartCoroutine(spellcaster.CastSlot(0, transform.position, playerTransform.position));
        }

        if ((playerTransform.position - transform.position).magnitude < 2f)
        {
            DoAttack();
        }
        else
        {
            GetComponent<Unit>().movement = direction.normalized * currentSpeed;
        }
    }

    protected virtual Vector3 GetMoveDirection()
    {
        if (Time.time >= nextPath)
        {
            nextPath = Time.time + repathTime;
            if (WaypointManager.Instance != null)
                path = new Queue<Vector3>(WaypointManager.Instance.FindPath(transform.position, playerTransform.position));
            else
            {
                path.Clear();
                path.Enqueue(playerTransform.position);
            }
        }

        while (path.Count > 0 && (path.Peek() - transform.position).magnitude < 0.1f)
            path.Dequeue();

        if (enemyType == "skeleton")
        {
            var all = Object.FindObjectsOfType<EnemyController>();
            Vector3 center = Vector3.zero;
            int count = 0;
            foreach (var e in all)
            {
                if (e == this) continue;
                if (e.enemyType == "skeleton" && Vector3.Distance(transform.position, e.transform.position) < 3f)
                {
                    center += e.transform.position;
                    count++;
                }
            }
            if (count > 0 && Vector3.Distance(transform.position, playerTransform.position) > 6f)
            {
                center /= count;
                return center - transform.position;
            }
        }

        if (path.Count > 0)
            return path.Peek() - transform.position;
        else
            return playerTransform.position - transform.position;
    }

    void DoAttack()
    {
        if (lastAttackTime + attackCooldown < Time.time)
        {
            lastAttackTime = Time.time;
            int amt = Mathf.RoundToInt(damage * damageModifier);
            playerTransform.gameObject.GetComponent<PlayerController>().hp.Damage(new Damage(amt, Damage.Type.PHYSICAL));
        }
    }

    // <param name="duration">Duration of slow in seconds.</param>
    // <param name="slowFactor">Multiplier to speed (0 to 1).</param>
    public void ApplySlow(float duration, float slowFactor)
    {
        StartCoroutine(SpeedModifierRoutine(duration, slowFactor));
    }
    public void ApplySpeedBuff(float duration, float factor)
    {
        StartCoroutine(SpeedModifierRoutine(duration, factor));
    }

    IEnumerator SpeedModifierRoutine(float duration, float factor)
    {
        speedModifier *= factor;
        currentSpeed = speed * speedModifier;
        yield return new WaitForSeconds(duration);
        speedModifier /= factor;
        currentSpeed = speed * speedModifier;
    }

    public void ApplyDamageBuff(float duration, float factor)
    {
        StartCoroutine(DamageBuffRoutine(duration, factor));
    }

    IEnumerator DamageBuffRoutine(float duration, float factor)
    {
        damageModifier *= factor;
        yield return new WaitForSeconds(duration);
        damageModifier /= factor;
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
