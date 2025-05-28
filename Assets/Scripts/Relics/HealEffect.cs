using UnityEngine;

public class HealEffect : RelicEffect
{
    private string amountExpr;

    public HealEffect(string expr)
    {
        amountExpr = expr;
    }

    public override void Apply(PlayerController player)
    {
        int amt = Mathf.RoundToInt(RPNEvaluator.Evaluate(amountExpr, new System.Collections.Generic.Dictionary<string, float>
        {
            {"wave", GameManager.Instance.currentWave }
        }));
        player.hp.hp = Mathf.Min(player.hp.hp + amt, player.hp.max_hp);
    }
}
