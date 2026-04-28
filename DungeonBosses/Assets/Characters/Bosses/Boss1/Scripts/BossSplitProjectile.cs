using UnityEngine;
using System.Collections;

public class BossSplitProjectile : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 6f;
    [SerializeField] private float lifetime = 6f;
    [SerializeField] private float splitDelay = 1.2f;

    [Header("Split")]
    [SerializeField] private int smallProjectileCount = 8;
    [SerializeField] private float smallProjectileDamage = 8f;
    [SerializeField] private float splitAngle = 70f;

    private BossProjectilePool pool;
    private Vector3 direction;
    private float damage;
    private bool hasSplit;
    private bool active;

    private Coroutine lifetimeRoutine;
    private Coroutine splitRoutine;

    public void Initialize(BossProjectilePool ownerPool, Vector3 dir, float projectileDamage)
    {
        pool = ownerPool;
        direction = dir.normalized;
        damage = projectileDamage;

        hasSplit = false;
        active = true;

        if (lifetimeRoutine != null)
            StopCoroutine(lifetimeRoutine);

        if (splitRoutine != null)
            StopCoroutine(splitRoutine);

        lifetimeRoutine = StartCoroutine(LifetimeRoutine());
        splitRoutine = StartCoroutine(SplitRoutine());
    }

    private void Update()
    {
        if (!active)
            return;

        transform.position += direction * speed * Time.deltaTime;
    }

    private IEnumerator LifetimeRoutine()
    {
        yield return new WaitForSeconds(lifetime);
        Despawn();
    }

    private IEnumerator SplitRoutine()
    {
        yield return new WaitForSeconds(splitDelay);
        Split();
    }

    private void Split()
    {
        if (!active || hasSplit || pool == null)
            return;

        hasSplit = true;

        for (int i = 0; i < smallProjectileCount; i++)
        {
            float t = smallProjectileCount == 1 ? 0.5f : i / (float)(smallProjectileCount - 1);
            float angle = Mathf.Lerp(-splitAngle * 0.5f, splitAngle * 0.5f, t);

            Vector3 spreadDir = Quaternion.AngleAxis(angle, Vector3.up) * direction;
            Quaternion rotation = Quaternion.LookRotation(spreadDir);

            BossSmallProjectile small = pool.GetSmallProjectile(transform.position, rotation);

            if (small != null)
                small.Initialize(pool, spreadDir, smallProjectileDamage);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!active)
            return;

        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

            if (playerHealth == null)
                playerHealth = other.GetComponentInParent<PlayerHealth>();

            if (playerHealth != null)
                playerHealth.TakeDamage(damage);

            Despawn();
        }
        else if (other.CompareTag("Wall"))
        {
            Despawn();
        }
    }

    private void Despawn()
    {
        if (!active)
            return;

        active = false;

        if (lifetimeRoutine != null)
        {
            StopCoroutine(lifetimeRoutine);
            lifetimeRoutine = null;
        }

        if (splitRoutine != null)
        {
            StopCoroutine(splitRoutine);
            splitRoutine = null;
        }

        if (pool != null)
            pool.ReturnSplitProjectile(this);
        else
            gameObject.SetActive(false);
    }
}