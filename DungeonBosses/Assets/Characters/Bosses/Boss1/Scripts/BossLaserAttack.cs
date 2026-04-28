using UnityEngine;
using System.Collections;

public class BossLaserAttack : BossAttack
{
    [Header("Laser")]
    [SerializeField] private GameObject laserPrefab;
    [SerializeField] private float warningTime = 0.75f;
    [SerializeField] private float laserDuration = 4f;
    [SerializeField] private float rotateDegrees = 90f;
    [SerializeField] private float laserDamagePerSecond = 25f;
    [SerializeField] private LayerMask wallMask;

    protected override IEnumerator AttackRoutine()
    {
        boss.StopMoving();

        yield return new WaitForSeconds(warningTime);

        if (boss.IsDead || boss.Target == null || laserPrefab == null || boss.LaserOrigin == null)
            yield break;

        Vector3 startDir = boss.Target.position - boss.LaserOrigin.position;
        startDir.y = 0f;
        startDir.Normalize();

        GameObject laserObj = Instantiate(
            laserPrefab,
            boss.LaserOrigin.position,
            Quaternion.LookRotation(startDir)
        );

        BossLaserBeam laser = laserObj.GetComponent<BossLaserBeam>();
        if (laser != null)
        {
            laser.Initialize(
                boss,
                boss.LaserOrigin,
                startDir,
                laserDuration,
                rotateDegrees,
                laserDamagePerSecond,
                wallMask
            );
        }

        yield return new WaitForSeconds(laserDuration);

        if (laserObj != null)
            Destroy(laserObj);
    }
}