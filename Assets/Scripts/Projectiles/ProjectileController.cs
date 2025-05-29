using UnityEngine;
using System;
using System.Collections;

public class ProjectileController : MonoBehaviour
{
    public float lifetime;
    public event Action<Hittable, Vector3> OnHit;
    public ProjectileMovement movement;
    public bool piercing = false;
    private bool hasHit = false; // Flag to check if it hit something

    void Start()
    {
        if (movement == null)
        {
            movement = GetComponent<ProjectileMovement>();
            if (movement == null)
            {
                Debug.LogWarning("ProjectileController: Missing ProjectileMovement component.");
            }
        }

        if (lifetime > 0)
        {
            StartCoroutine(Expire());
        }
    }

    void Update()
    {
        if (movement != null)
        {
            movement.Movement(transform);
        }
        else
        {
            Debug.LogWarning("ProjectileController: Cannot move projectile, movement is null.");
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("projectile")) return;

        hasHit = true; // Mark that the projectile has hit something
        if (collision.gameObject.CompareTag("unit"))
        {
            var ec = collision.gameObject.GetComponent<EnemyController>();
            if (ec != null && OnHit != null)
            {
                OnHit.Invoke(ec.hp, transform.position);
            }
            else
            {
                var pc = collision.gameObject.GetComponent<PlayerController>();
                if (pc != null && OnHit != null)
                {
                    OnHit.Invoke(pc.hp, transform.position);
                }
            }
        }

        if (!piercing) 
            Destroy(gameObject);
    }

    public void SetLifetime(float newLifetime)
    {
        lifetime = newLifetime;
        StopAllCoroutines();
        if (lifetime > 0)
            StartCoroutine(Expire());
    }

    IEnumerator Expire()
    {
        yield return new WaitForSeconds(lifetime);
        if (!hasHit)
        {
            // If lifetime expires and it hasn't hit anything, it's a miss
            RelicEventBus.SpellMissed();
        }
        Destroy(gameObject);
    }
}