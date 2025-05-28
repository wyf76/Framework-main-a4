using UnityEngine;

public class GainManaEffect : RelicEffect
{
    private string amountExpr;

    public GainManaEffect(string expr)
    {
        amountExpr = expr;
    }

    public override void Apply(PlayerController player)
    {
        int amt = Mathf.RoundToInt(RPNEvaluator.Evaluate(amountExpr, new System.Collections.Generic.Dictionary<string, float>
        {
            {"wave", GameManager.Instance.currentWave }
        }));
        player.spellcaster.mana = Mathf.Min(player.spellcaster.mana + amt, player.spellcaster.max_mana);
    }
}
