using UnityEngine;

public class GainSpellpowerEffect : RelicEffect
{
    private string amountExpr;
    private string untilEvent;
    private bool active;
    private int lastGain;
    private PlayerController player;

    public GainSpellpowerEffect(string expr, string until)
    {
        amountExpr = expr;
        untilEvent = until;
    }

    public override void Apply(PlayerController player)
    {
        this.player = player;
        int amt = Mathf.RoundToInt(RPNEvaluator.Evaluate(amountExpr, new System.Collections.Generic.Dictionary<string, float>
        {
            {"wave", GameManager.Instance.currentWave }
        }));
        player.spellcaster.spellPower += amt;
        lastGain = amt;
        active = true;
        if (!string.IsNullOrEmpty(untilEvent))
        {
            switch (untilEvent)
            {
                case "move":
                    EventBus.Instance.OnPlayerMove += End;
                    break;
                case "cast-spell":
                    EventBus.Instance.OnSpellCast += End;
                    break;
            }
        }
    }

    private void End(float _)
    {
        End();
    }

    private void End()
    {
        if (!active) return;
        if (player != null)
            player.spellcaster.spellPower -= lastGain;
        if (!string.IsNullOrEmpty(untilEvent))
        {
            switch (untilEvent)
            {
                case "move":
                    EventBus.Instance.OnPlayerMove -= End;
                    break;
                case "cast-spell":
                    EventBus.Instance.OnSpellCast -= End;
                    break;
            }
        }
        active = false;
    }

    public override void Remove(PlayerController player)
    {
        End();
    }
}
