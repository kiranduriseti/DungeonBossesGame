using UnityEngine;
using System.Collections;

public class BossBigSplitProjectileAttack : BossAttack
{
    [Header("Big Split Projectile")]
    [SerializeField] private BossProjectilePool projectilePool;
    [SerializeField] private float fireDelay = 0.4f;
    [SerializeField] private float damage = 20f;
    [SerializeField] private float aimHeight = 1.2f;

    protected override IEnumerator AttackRoutine()
    {
        boss.StopMoving();
        boss.FaceTarget();

        yield return new WaitForSeconds(fireDelay);

        if (boss.IsDead || boss.Target == null || projectilePool == null || boss.ProjectileSpawnPoint == null)
            yield break;

        Vector3 aimPoint = boss.Target.position + Vector3.up * aimHeight;
        Vector3 dir = (aimPoint - boss.ProjectileSpawnPoint.position).normalized;
        Quaternion rotation = Quaternion.LookRotation(dir);

        BossSplitProjectile projectile = projectilePool.GetSplitProjectile(
            boss.ProjectileSpawnPoint.position,
            rotation
        );

        if (projectile != null)
            projectile.Initialize(projectilePool, dir, damage);
    }
}