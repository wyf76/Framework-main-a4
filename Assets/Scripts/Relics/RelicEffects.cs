using System;
using System.Collections.Generic;
using UnityEngine;

public interface IRelicEffect
{
    void Activate();
    void Deactivate();
}

public static class RelicEffects
{
    public static IRelicEffect Create(EffectData d, Relic r)
    {
        if (d.type == "gain-mana")
            return new GainMana(int.Parse(d.amount), r.Name);

        if (d.type == "gain-spellpower")
        {
            if (d.until == "cast-spell")
                return new GainSpellPowerOnce(int.Parse(d.amount), r.Name);
            if (d.until == "move")
                return new GainSpellPowerUntilMove(d.amount, r.Name);
        }

        if (d.type == "reduce-mana-cost")
            return new ReduceManaCost(int.Parse(d.amount), r.Name);
            
        if (d.type == "auto-cast-retaliation")
            return new AutoCastRetaliation(r);
            
        if (d.type == "heal") // Ensures "heal" is handled
            return new HealEffect(int.Parse(d.amount), r.Name);

        throw new Exception($"Unknown effect type: {d.type}");
    }

    // Definition for HealEffect
    class HealEffect : IRelicEffect
    {
        readonly int amt;
        readonly string relicName;
        public HealEffect(int a, string name) { amt = a; relicName = name; }
        public void Activate()
        {
            var pc = GameManager.Instance.player.GetComponent<PlayerController>();
            if(pc != null && pc.hp != null)
            {
                pc.hp.Heal(amt);
            }
        }
        public void Deactivate() { } // Heal is a one-time effect
    }

    class GainMana : IRelicEffect
    {
        readonly int amt;
        readonly string relicName;
        public GainMana(int a, string name) { amt = a; relicName = name; }
        public void Activate()
        {
            var pc = GameManager.Instance.player.GetComponent<PlayerController>();
            pc.GainMana(amt);
        }
        public void Deactivate() { }
    }

    class GainSpellPowerOnce : IRelicEffect
    {
        readonly int amt;
        readonly string relicName;
        bool pending = false;

        public GainSpellPowerOnce(int a, string name) { amt = a; relicName = name; }

        public void Activate()
        {
            if (pending) return;
            pending = true;

            var pc = GameManager.Instance.player.GetComponent<PlayerController>();
            pc.AddSpellPower(amt);
            SpellCaster.OnSpellCast += HandleSpellCast;
        }

        void HandleSpellCast()
        {
            if (!pending) return;
            var pc = GameManager.Instance.player.GetComponent<PlayerController>();
            pc.AddSpellPower(-amt);
            pending = false;
            SpellCaster.OnSpellCast -= HandleSpellCast;
        }

        public void Deactivate()
        {
            if (pending)
            {
                SpellCaster.OnSpellCast -= HandleSpellCast;
                pending = false;
            }
        }
    }

    class GainSpellPowerUntilMove : IRelicEffect
    {
        readonly string formula;
        readonly string relicName;
        int buffAmt = 0;
        bool active = false;

        public GainSpellPowerUntilMove(string f, string name) { formula = f; relicName = name; }

        public void Activate()
        {
            if (active) return;
            var vars = new Dictionary<string, int> { { "wave", GameManager.Instance.wavesCompleted } };
            buffAmt = RPNEvaluator.Evaluate(formula, vars); //
            GameManager.Instance.player.GetComponent<PlayerController>().AddSpellPower(buffAmt); //
            active = true;
        }

        public void Deactivate()
        {
            if (!active) return;
            GameManager.Instance.player.GetComponent<PlayerController>().AddSpellPower(-buffAmt); //
            active = false;
        }
    }

    class ReduceManaCost : IRelicEffect
    {
        readonly int amt;
        readonly string relicName;
        public ReduceManaCost(int a, string name) { amt = a; relicName = name; }

        public void Activate()
        {
            PlayerController pc = GameManager.Instance.player.GetComponent<PlayerController>(); //
            if (pc != null && pc.spellcaster != null)
            {
                pc.spellcaster.nextSpellManaDiscount += amt; //
            }
        }

        public void Deactivate() { }
    }
    
    class AutoCastRetaliation : IRelicEffect
    {
        readonly Relic ownerRelic;
        private float cooldown;
        private float lastActivationTime = -999f;

        public AutoCastRetaliation(Relic relic)
        {
            ownerRelic = relic;
            // Try to parse the cooldown, default to 10 if it's missing or invalid
            float.TryParse(relic.EffectData.cooldown, out this.cooldown); //
            if (this.cooldown <= 0) this.cooldown = 10f;
        }

        public void Activate()
        {
            // When the relic is picked up, start listening for damage events.
            EventBus.Instance.OnDamage += OnPlayerDamaged; //
        }

        public void Deactivate()
        {
            // When the relic is lost (if that's a feature), stop listening.
            EventBus.Instance.OnDamage -= OnPlayerDamaged; //
        }

        private void OnPlayerDamaged(Vector3 pos, Damage dmg, Hittable target)
        {
            // Check 1: Was the player the one who got hit?
            if (target.team != Hittable.Team.PLAYER) //
            {
                return;
            }

            // Check 2: Is the relic off cooldown?
            if (Time.time < lastActivationTime + cooldown)
            {
                return;
            }

            // Check 3: Find a target to retaliate against.
            GameObject enemyToAttack = GameManager.Instance.GetClosestEnemy(target.owner.transform.position); //
            if (enemyToAttack == null)
            {
                return; // No enemies to retaliate against.
            }

            // All checks passed, let's retaliate!
            Debug.Log($"[RelicEffect] “{ownerRelic.Name}” retaliating against {enemyToAttack.name}"); //

            var playerController = target.owner.GetComponent<PlayerController>(); //
            if (playerController != null && playerController.spellcaster != null)
            {
                // Cast the first spell (slot 0)
                playerController.StartCoroutine(
                    playerController.spellcaster.CastSlot(0,  //
                        playerController.transform.position, 
                        enemyToAttack.transform.position
                    )
                );

                // Put the relic on cooldown.
                lastActivationTime = Time.time;
            }
        }
    }
}