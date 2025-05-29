using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

public sealed class HomingModifier : ModifierSpell
{
    private float _damageMultiplier;
    private float _manaAdder;
    private string _suffix;

    public HomingModifier(Spell inner) : base(inner)
    {
        _damageMultiplier = 0.75f;
        _manaAdder        = 10f;
        _suffix           = "homing";
    }

    protected override string Suffix => _suffix;

    public override void LoadAttributes(JObject json, Dictionary<string, float> vars)
    {
        _suffix = json["name"]?.Value<string>() ?? _suffix;

        if (json["damage_multiplier"] != null)
            _damageMultiplier = RPNEvaluator.SafeEvaluateFloat(json["damage_multiplier"].Value<string>(), vars, _damageMultiplier);

        if (json["mana_adder"] != null)
            _manaAdder = RPNEvaluator.SafeEvaluateFloat(json["mana_adder"].Value<string>(), vars, _manaAdder);

        base.LoadAttributes(json, vars);
    }

    protected override void InjectMods(StatBlock mods)
    {
        mods.DamageMods.Add(new ValueMod(ModOp.Mul, _damageMultiplier));
        mods.ManaMods.Add(new ValueMod(ModOp.Add, _manaAdder));
    }

    protected override IEnumerator ApplyModifierEffect(Vector3 origin, Vector3 target)
    {
        // Choose pattern based on inner type
        if (inner is ArcaneSpray)      yield return CreateHomingSpray(origin, target);
        else if (inner is ArcaneBlast) yield return CreateHomingBlast(origin, target);
        else                            yield return CreateGenericHoming(origin, target);
    }

    private IEnumerator CreateHomingSpray(Vector3 o, Vector3 t)
    {
        Vector3 dir = (t - o).normalized;
        float  ang = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        int    cnt = Mathf.Max(1, Mathf.RoundToInt(inner.Damage) + 5);
        float  span = 60f;
        float  step = span / (cnt - 1);
        float  start = ang - span/2;

        for (int i = 0; i < cnt; i++)
        {
            float  a   = start + step * i;
            Vector3 d  = new Vector3(Mathf.Cos(a*Mathf.Deg2Rad), Mathf.Sin(a*Mathf.Deg2Rad),0);
            GameObject enemy = GameManager.Instance.GetClosestEnemy(o);
            Vector3   tgt   = enemy? (enemy.transform.position-o).normalized : d;

            GameManager.Instance.projectileManager.CreateProjectile(
                inner.IconIndex, "homing", o, tgt, inner.Speed,
                (hit,pos)=> { if(hit.team!=owner.team) hit.Damage(new global::Damage(Mathf.RoundToInt(Damage), global::Damage.Type.ARCANE)); },
                0.1f + inner.Speed/40f
            );
            yield return new WaitForSeconds(0.02f);
        }
    }

    private IEnumerator CreateHomingBlast(Vector3 o, Vector3 t)
    {
        GameObject enemy = GameManager.Instance.GetClosestEnemy(o);
        Vector3   dir   = enemy? (enemy.transform.position-o).normalized : (t-o).normalized;

        GameManager.Instance.projectileManager.CreateProjectile(
            inner.IconIndex, "homing", o, dir, inner.Speed,
            (hit,pos)=>{
                if (hit.team!=owner.team)
                {
                    hit.Damage(new global::Damage(Mathf.RoundToInt(Damage), global::Damage.Type.ARCANE));
                    CreateHomingSecondary(pos, Mathf.RoundToInt(Damage)/4);
                }
            }
        );
        yield return null;
    }

    private void CreateHomingSecondary(Vector3 c, int dmg)
    {
        int cnt = 8; float step=360f/cnt;
        for(int i=0;i<cnt;i++){
            float a = i*step;
            Vector3 d = new Vector3(Mathf.Cos(a*Mathf.Deg2Rad), Mathf.Sin(a*Mathf.Deg2Rad),0);
            GameManager.Instance.projectileManager.CreateProjectile(
                inner.IconIndex, "homing", c, d, inner.Speed*0.8f,
                (hit,pos)=>{ if(hit.team!=owner.team) hit.Damage(new global::Damage(dmg, global::Damage.Type.ARCANE)); },
                0.3f
            );
        }
    }

    private IEnumerator CreateGenericHoming(Vector3 o, Vector3 t)
    {
        GameObject enemy = GameManager.Instance.GetClosestEnemy(o);
        Vector3   dir   = (enemy? enemy.transform.position : t - o).normalized;

        GameManager.Instance.projectileManager.CreateProjectile(
            inner.IconIndex, "homing", o, dir, inner.Speed,
            (hit,pos)=>{ if(hit.team!=owner.team) hit.Damage(new global::Damage(Mathf.RoundToInt(Damage), global::Damage.Type.ARCANE)); }
        );
        yield return null;
    }
}