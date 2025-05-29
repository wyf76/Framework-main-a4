using UnityEngine;
using System;

public class ProjectileManager : MonoBehaviour
{
    public GameObject[] projectiles;

    void Start()
    {
        GameManager.Instance.projectileManager = this;
    }

    public void CreateProjectile(int idx, string traj, Vector3 pos, Vector3 dir, float spd, Action<Hittable,Vector3> onHit, float life=0f)
    {
        int i = Mathf.Clamp(idx, 0, projectiles.Length-1);
        var go = Instantiate(projectiles[i], pos + dir.normalized*1.1f,
                             Quaternion.Euler(0,0,Mathf.Atan2(dir.y,dir.x)*Mathf.Rad2Deg));
        var mov = MakeMovement(traj, spd);
        var ctrl= go.GetComponent<ProjectileController>();
        ctrl.movement = mov;
        ctrl.OnHit    += onHit;
        if (life>0) ctrl.SetLifetime(life);
    }

    private ProjectileMovement MakeMovement(string name, float speed)
    {
        if (string.IsNullOrEmpty(name)) name = "straight";
        switch(name.ToLower())
        {
            case "homing":    return new HomingProjectileMovement(speed);
            case "spiraling": return new SpiralingProjectileMovement(speed);
            default:           return new StraightProjectileMovement(speed);
        }
    }
}