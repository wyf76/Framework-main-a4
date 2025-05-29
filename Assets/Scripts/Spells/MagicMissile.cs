using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

public sealed class MagicMissile : Spell
{
    private string _displayName;
    private string _trajectory;
    private int _sprite;
    private float _baseMana;
    private float _baseCD;
    private string _damageExpr;
    private string _speedExpr;

    public MagicMissile(SpellCaster owner) : base(owner) { }

    public override string DisplayName => _displayName;
    public override int IconIndex    => _sprite;

    protected override float BaseDamage
    {
        get
        {
            float pw = owner.spellPower;
            float wv = GetWave();
            return RPNEvaluator.SafeEvaluateFloat(_damageExpr, new Dictionary<string,float>{{"power",pw},{"wave",wv}},10f);
        }
    }
    protected override float BaseSpeed  => RPNEvaluator.SafeEvaluateFloat(_speedExpr,new Dictionary<string,float>{{"power",owner.spellPower},{"wave",GetWave()}},10f);
    protected override float BaseMana   => _baseMana;
    protected override float BaseCooldown=> _baseCD;

    private float GetWave()
    {
        var sp=Object.FindFirstObjectByType<EnemySpawnerController>();
        return sp? sp.currentWave:1f;
    }

    public override void LoadAttributes(JObject json, Dictionary<string, float> vars)
    {
        _displayName = json["name"].Value<string>();
        _sprite      = json["icon"].Value<int>();
        _damageExpr  = json["damage"]["amount"].Value<string>();
        _speedExpr   = json["projectile"]["speed"].Value<string>();
        _baseMana    = RPNEvaluator.SafeEvaluateFloat(json["mana_cost"].Value<string>(), vars, 1f);
        _baseCD      = RPNEvaluator.SafeEvaluateFloat(json["cooldown"].Value<string>(), vars, 0f);
        _trajectory  = json["projectile"]["trajectory"].Value<string>();
    }

    protected override IEnumerator Cast(Vector3 o, Vector3 t)
    {
        float dmg = Damage;
        float spd = Speed;
        GameObject enemy = GameManager.Instance.GetClosestEnemy(o);
        Vector3 dir = (enemy? enemy.transform.position : t - o).normalized;
        GameManager.Instance.projectileManager.CreateProjectile(_sprite,_trajectory,o,dir,spd,
            (hit,pos)=> { if(hit.team!=owner.team) hit.Damage(new global::Damage(Mathf.RoundToInt(dmg),global::Damage.Type.ARCANE)); });
        yield return null;
    }
}
