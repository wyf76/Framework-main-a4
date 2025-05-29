using System;
using UnityEngine;
using System.Collections;

public interface IRelicTrigger
{
    void Subscribe();
    void Unsubscribe();
}

public static class RelicTriggers
{
    public static IRelicTrigger Create(TriggerData d, Relic r) => d.type switch
    {
        "take-damage" => new DamageTrigger(r),
        "deal-damage" => new DealDamageTrigger(r), // Ensures "deal-damage" is handled
        "on-kill" => new KillTrigger(r),
        "stand-still" => new StandStillTrigger(r, float.Parse(d.amount)),
        "on-move-distance" => new MoveDistanceTrigger(r, float.Parse(d.amount)),
        "on-miss" => new MissTrigger(r),
        _ => throw new Exception($"Unknown trigger type: {d.type}")
    };

    class DamageTrigger : IRelicTrigger
    {
        readonly Relic relic;
        public DamageTrigger(Relic r) { relic = r; }

        void HandleDamage(Vector3 pos, Damage dmg, Hittable target)
        {
            if (target.team == Hittable.Team.PLAYER)
            {
                relic.Fire();
            }
        }

        public void Subscribe()
        {
            EventBus.Instance.OnDamage += HandleDamage;
        }

        public void Unsubscribe()
        {
            EventBus.Instance.OnDamage -= HandleDamage;
        }
    }

    // Definition for DealDamageTrigger
    class DealDamageTrigger : IRelicTrigger
    {
        readonly Relic relic;
        public DealDamageTrigger(Relic r) { relic = r; }

        void HandleDamage(Vector3 pos, Damage dmg, Hittable target)
        {
            // Assuming only the player can damage monsters,
            // so if a monster is hit, it's by the player.
            if (target.team == Hittable.Team.MONSTERS)
            {
                relic.Fire();
            }
        }

        public void Subscribe()
        {
            EventBus.Instance.OnDamage += HandleDamage;
        }

        public void Unsubscribe()
        {
            EventBus.Instance.OnDamage -= HandleDamage;
        }
    }

    class KillTrigger : IRelicTrigger
    {
        readonly Relic relic;
        public KillTrigger(Relic r) { relic = r; }

        void OnKilled(GameObject enemy)
        {
            relic.Fire();
        }

        public void Subscribe()
        {
            EnemySpawnerController.OnEnemyKilled += OnKilled;
        }

        public void Unsubscribe()
        {
            EnemySpawnerController.OnEnemyKilled -= OnKilled;
        }
    }

    class StandStillTrigger : IRelicTrigger
    {
        readonly Relic relic;
        readonly float secs;
        bool buffActive = false;
        Coroutine watcher = null;

        public StandStillTrigger(Relic r, float amount)
        {
            relic = r;
            secs = amount;
        }

        public void Subscribe()
        {
            PlayerController.OnPlayerMove += HandleMove;
            var pc = GameManager.Instance.player.GetComponent<PlayerController>();
            if (pc.unit.movement.sqrMagnitude < 0.01f)
            {
                StartWatcher(pc);
            }
        }

        public void Unsubscribe()
        {
            PlayerController.OnPlayerMove -= HandleMove;
            StopWatcher();
        }

        void HandleMove(Vector3 v)
        {
            var pc = GameManager.Instance.player.GetComponent<PlayerController>();
            if (v.sqrMagnitude < 0.01f)
            {
                if (!buffActive && watcher == null)
                {
                    StartWatcher(pc);
                }
            }
            else
            {
                if (watcher != null)
                {
                    StopWatcher();
                }
                if (buffActive)
                {
                    relic.End();
                    buffActive = false;
                }
            }
        }

        void StartWatcher(PlayerController pc)
        {
            watcher = CoroutineManager.Instance.StartCoroutine(CheckStandStill(pc));
        }

        void StopWatcher()
        {
            if (watcher != null)
            {
                CoroutineManager.Instance.StopCoroutine(watcher);
                watcher = null;
            }
        }

        IEnumerator CheckStandStill(PlayerController pc)
        {
            yield return new WaitForSeconds(secs);
            if (pc.unit.movement.sqrMagnitude < 0.01f && !buffActive)
            {
                relic.Fire();
                buffActive = true;
            }
            watcher = null;
        }
    }

    class MoveDistanceTrigger : IRelicTrigger
    {
        readonly Relic relic;
        readonly float distanceNeeded;
        float accumulated = 0f;

        public MoveDistanceTrigger(Relic r, float d)
        {
            relic = r;
            distanceNeeded = d;
        }

        void OnDistanceMoved(float distance)
        {
            accumulated += distance;
            if (accumulated >= distanceNeeded)
            {
                relic.Fire();
                accumulated -= distanceNeeded;
            }
        }

        public void Subscribe()
        {
            RelicEventBus.OnPlayerMovedDistance += OnDistanceMoved;
        }

        public void Unsubscribe()
        {
            RelicEventBus.OnPlayerMovedDistance -= OnDistanceMoved;
        }
    }
    
    class MissTrigger : IRelicTrigger
    {
        readonly Relic relic;
        public MissTrigger(Relic r) { relic = r; }

        void HandleMiss()
        {
            relic.Fire();
        }

        public void Subscribe()
        {
            RelicEventBus.OnSpellMissed += HandleMiss;
        }

        public void Unsubscribe()
        {
            RelicEventBus.OnSpellMissed -= HandleMiss;
        }
    }
}