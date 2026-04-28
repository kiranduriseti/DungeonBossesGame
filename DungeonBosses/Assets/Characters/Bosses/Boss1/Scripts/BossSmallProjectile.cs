using UnityEngine;
using System.Collections;

public class BossSmallProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 12f;
    [SerializeField] private float lifetime = 5f;

    private BossProjectilePool pool;
    private Vector3 direction;
    private float damage;
    private Coroutine lifetimeRoutine;
    private bool active;

    public void Initialize(BossProjectilePool ownerPool, Vector3 dir, float projectileDamage)
    {
        pool = ownerPool;
        direction = dir.normalized;
        damage = projectileDamage;
        active = true;

        if (lifetimeRoutine != null)
            StopCoroutine(lifetimeRoutine);

        lifetimeRoutine = StartCoroutine(LifetimeRoutine());
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

        if (pool != null)
            pool.ReturnSmallProjectile(this);
        else
            gameObject.SetActive(false);
    }
}