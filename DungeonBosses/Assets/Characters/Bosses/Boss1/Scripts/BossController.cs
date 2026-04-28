using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class BossController : MonoBehaviour
{
    [Header("Stats")]
    public float health = 300f;
    public float aggroRadius = 40f;
    public float attackRadius = 30f;
    public float moveSpeed = 3.5f;
    public float attackCooldown = 2f;

    [Header("Detection")]
    [SerializeField] private LayerMask playerMask;

    [Header("References")]
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private Transform laserOrigin;
    [SerializeField] private Transform dashPoint;

    private float maxHealth;
    private bool isDead;
    private bool isAttacking;
    private bool isInvulnerable;

    private Transform target;
    private NavMeshAgent agent;
    private Animator anim;

    private BossAttack[] attacks;

    public Transform Target => target;
    public Transform ProjectileSpawnPoint => projectileSpawnPoint;
    public Transform LaserOrigin => laserOrigin;
    public bool IsDead => isDead;
    public bool IsInvulnerable => isInvulnerable;

    public event System.Action<float, float> OnHealthChanged;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();

        attacks = GetComponents<BossAttack>();

        foreach (BossAttack attack in attacks)
        {
            attack.Initialize(this);
        }
    }

    private void Start()
    {
        maxHealth = health;

        if (agent != null)
            agent.speed = moveSpeed;
    }

    private void Update()
    {
        if (isDead)
            return;

        if (target == null)
        {
            FindPlayer();
            return;
        }

        if (PlayerIsDead())
        {
            target = null;
            StopMoving();
            return;
        }

        FaceTarget();

        float distance = Vector3.Distance(transform.position, target.position);

        if (!isAttacking)
        {
            if (distance > attackRadius)
            {
                ChasePlayer();
            }
            else
            {
                StopMoving();
                StartCoroutine(AttackRoutine());
            }
        }

        if (anim != null && agent != null && agent.speed > 0f)
        {
            anim.SetFloat("Speed", agent.velocity.magnitude / agent.speed);
        }
    }

    private void FindPlayer()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, aggroRadius, playerMask);

        foreach (Collider hit in hits)
        {
            if (!hit.CompareTag("Player"))
                continue;

            PlayerController player = hit.GetComponent<PlayerController>();
            //PlayerHealth playerHealth = hit.GetComponent<PlayerController>();
            if (player == null)
                player = hit.GetComponentInParent<PlayerController>();

            // if (player != null && playerHealth.GetCurrentHealth() > 0f)
            // {
            //     target = player.transform;
            //     break;
            // }
            if (player != null && !PlayerIsDead())
            {
                target = player.transform;
                break;
            }
        }
    }

    private bool PlayerIsDead()
    {
        if (target == null)
            return true;

        PlayerHealth playerHealth = target.GetComponent<PlayerHealth>();
        PlayerController player = target.GetComponent<PlayerController>();

        if (player == null)
            player = target.GetComponentInParent<PlayerController>();

        return player == null || playerHealth.GetCurrentHealth() <= 0f;
    }

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;

        BossAttack attack = ChooseAttack();

        if (attack != null)
        {
            yield return StartCoroutine(attack.Execute());
        }

        yield return new WaitForSeconds(attackCooldown);

        isAttacking = false;
    }

    private BossAttack ChooseAttack()
    {
        List<BossAttack> usableAttacks = new List<BossAttack>();

        foreach (BossAttack attack in attacks)
        {
            if (attack.CanUse())
                usableAttacks.Add(attack);
        }

        if (usableAttacks.Count == 0)
            return null;

        return usableAttacks[Random.Range(0, usableAttacks.Count)];
    }

    private void ChasePlayer()
    {
        if (agent == null || !agent.isOnNavMesh || target == null)
            return;

        agent.isStopped = false;
        agent.SetDestination(target.position);
    }

    public void StopMoving()
    {
        if (agent == null || !agent.isOnNavMesh)
            return;

        agent.isStopped = true;
        agent.ResetPath();
    }

    public void FaceTarget()
    {
        if (target == null)
            return;

        Vector3 dir = target.position - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 8f * Time.deltaTime);
    }

    public void SetInvulnerable(bool value)
    {
        isInvulnerable = value;
    }

    public void TakeDamage(float damage)
    {
        if (isDead || isInvulnerable)
            return;

        health -= damage;
        health = Mathf.Max(health, 0f);

        OnHealthChanged?.Invoke(maxHealth, health);

        if (health <= 0f)
            Die();
    }

    public void enemyhit(float damage = 10f)
    {
        TakeDamage(damage);
    }

    private void Die()
    {
        isDead = true;
        isAttacking = false;

        StopMoving();

        if (anim != null)
            anim.SetTrigger("Die");

        if (GameManager.Instance != null)
            GameManager.Instance.AddKill();

        Collider col = GetComponent<Collider>();
        if (col != null)
            col.enabled = false;

        Destroy(gameObject, 4f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isDead)
            return;

        if (other.CompareTag("Spell"))
        {
            enemyhit();
        }
    }
}