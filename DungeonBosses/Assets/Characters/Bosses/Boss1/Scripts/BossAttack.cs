using UnityEngine;
using System.Collections;

public abstract class BossAttack : MonoBehaviour
{
    [Header("Attack Base")]
    public float cooldown = 3f;
    public float range = 30f;

    protected BossController boss;
    private float lastUseTime = -999f;

    public virtual void Initialize(BossController bossController)
    {
        boss = bossController;
    }

    public virtual bool CanUse()
    {
        if (boss == null || boss.Target == null)
            return false;

        float distance = Vector3.Distance(transform.position, boss.Target.position);

        return Time.time >= lastUseTime + cooldown && distance <= range;
    }

    public IEnumerator Execute()
    {
        lastUseTime = Time.time;
        yield return StartCoroutine(AttackRoutine());
    }

    protected abstract IEnumerator AttackRoutine();
}