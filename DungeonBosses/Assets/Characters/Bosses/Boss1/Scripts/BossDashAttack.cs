using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class BossDashAttack : BossAttack
{
    [Header("Dash")]
    [SerializeField] private float windupTime = 0.5f;
    [SerializeField] private float dashSpeed = 20f;
    [SerializeField] private float dashDuration = 0.35f;
    [SerializeField] private float dashDamage = 30f;
    [SerializeField] private float invulnerableExtraTime = 0.25f;

    [Header("Collision")]
    [SerializeField] private float hitRadius = 1.5f;
    [SerializeField] private LayerMask playerMask;

    private bool hasHitPlayer;

    protected override IEnumerator AttackRoutine()
    {
        boss.StopMoving();
        boss.FaceTarget();

        hasHitPlayer = false;

        yield return new WaitForSeconds(windupTime);

        if (boss.IsDead || boss.Target == null)
            yield break;

        NavMeshAgent agent = GetComponent<NavMeshAgent>();

        if (agent != null)
            agent.enabled = false;

        boss.SetInvulnerable(true);

        Vector3 dashDir = boss.Target.position - transform.position;
        dashDir.y = 0f;

        if (dashDir.sqrMagnitude < 0.001f)
            dashDir = transform.forward;

        dashDir.Normalize();

        float timer = 0f;

        while (timer < dashDuration && !boss.IsDead)
        {
            transform.position += dashDir * dashSpeed * Time.deltaTime;
            CheckDashHit();

            timer += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(invulnerableExtraTime);

        boss.SetInvulnerable(false);

        if (agent != null)
        {
            agent.enabled = true;

            if (agent.isOnNavMesh)
                agent.Warp(transform.position);
        }
    }

    private void CheckDashHit()
    {
        if (hasHitPlayer)
            return;

        Collider[] hits = Physics.OverlapSphere(transform.position, hitRadius, playerMask);

        foreach (Collider hit in hits)
        {
            PlayerHealth playerHealth = hit.GetComponent<PlayerHealth>();

            if (playerHealth == null)
                playerHealth = hit.GetComponentInParent<PlayerHealth>();

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(dashDamage);
                hasHitPlayer = true;
                return;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, hitRadius);
    }
}