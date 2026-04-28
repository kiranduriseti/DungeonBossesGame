using UnityEngine;
using System.Collections.Generic;

public class BossProjectilePool : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject splitProjectilePrefab;
    [SerializeField] private GameObject smallProjectilePrefab;

    [Header("Pool Sizes")]
    [SerializeField] private int splitProjectilePoolSize = 10;
    [SerializeField] private int smallProjectilePoolSize = 80;

    private readonly Queue<GameObject> splitProjectiles = new Queue<GameObject>();
    private readonly Queue<GameObject> smallProjectiles = new Queue<GameObject>();

    private void Awake()
    {
        Preload(splitProjectilePrefab, splitProjectiles, splitProjectilePoolSize);
        Preload(smallProjectilePrefab, smallProjectiles, smallProjectilePoolSize);
    }

    private void Preload(GameObject prefab, Queue<GameObject> pool, int count)
    {
        if (prefab == null)
            return;

        for (int i = 0; i < count; i++)
        {
            GameObject obj = Instantiate(prefab, transform);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    public BossSplitProjectile GetSplitProjectile(Vector3 position, Quaternion rotation)
    {
        GameObject obj = GetObject(splitProjectilePrefab, splitProjectiles, position, rotation);

        if (obj == null)
            return null;

        return obj.GetComponent<BossSplitProjectile>();
    }

    public BossSmallProjectile GetSmallProjectile(Vector3 position, Quaternion rotation)
    {
        GameObject obj = GetObject(smallProjectilePrefab, smallProjectiles, position, rotation);

        if (obj == null)
            return null;

        return obj.GetComponent<BossSmallProjectile>();
    }

    private GameObject GetObject(GameObject prefab, Queue<GameObject> pool, Vector3 position, Quaternion rotation)
    {
        if (prefab == null)
            return null;

        GameObject obj = pool.Count > 0 ? pool.Dequeue() : Instantiate(prefab, transform);

        obj.transform.SetPositionAndRotation(position, rotation);
        obj.SetActive(true);

        return obj;
    }

    public void ReturnSplitProjectile(BossSplitProjectile projectile)
    {
        if (projectile == null)
            return;

        GameObject obj = projectile.gameObject;
        obj.SetActive(false);
        obj.transform.SetParent(transform);
        splitProjectiles.Enqueue(obj);
    }

    public void ReturnSmallProjectile(BossSmallProjectile projectile)
    {
        if (projectile == null)
            return;

        GameObject obj = projectile.gameObject;
        obj.SetActive(false);
        obj.transform.SetParent(transform);
        smallProjectiles.Enqueue(obj);
    }
}