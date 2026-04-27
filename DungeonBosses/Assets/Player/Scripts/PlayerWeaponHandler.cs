using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class PlayerWeaponHandler : MonoBehaviour
{
    [Header("Weapons")]
    [SerializeField] private PlayerGun primaryGun;
    [SerializeField] private PlayerGun secondaryGun;
    [SerializeField] private Transform gunHoldParent;

    [Header("Shooting")]
    [SerializeField] private float shootRange = 100f;
    [SerializeField] private LayerMask shootMask = ~0;

    [Header("Effects")]
    [SerializeField] private GameObject bulletHolePrefab;
    [SerializeField] private float bulletHoleLifetime = 5f;
    [SerializeField] private float muzzleFlashLifetime = 0.1f;

    [Header("Events")]
    public UnityEvent<PlayerGun> OnWeaponChanged;
    public UnityEvent<PlayerGun> OnSpendAmmoRequested;
    public UnityEvent<PlayerGun> OnAmmoCheckRequested;
    public UnityEvent OnAmmoEmpty;
    public UnityEvent OnShootAnimationRequested;

    private bool usingPrimary = true;
    private bool canShoot = true;
    private bool isSprinting = false;
    private bool hasAmmo = true;
    private float attackSpeedMultiplier = 1f;

    private void Start()
    {
        InitializeGun(primaryGun);
        InitializeGun(secondaryGun);
        EquipPrimary();
    }

    private void InitializeGun(PlayerGun gun)
    {
        if (gun == null)
            return;

        gun.InitializeWeapon();
    }

    public void FireCurrentWeapon()
    {
        if (!canShoot || isSprinting)
            return;

        PlayerGun gun = GetCurrentGun();

        if (gun == null)
            return;

        if (!hasAmmo)
        {
            OnAmmoEmpty?.Invoke();
            return;
        }

        SpawnMuzzleFlash(gun);
        ShootRay(gun);

        gun.Fire();

        OnShootAnimationRequested?.Invoke();
        OnSpendAmmoRequested?.Invoke(gun);

        StartCoroutine(ShootCooldown(gun.GetFireRate() / attackSpeedMultiplier));
    }

    private void ShootRay(PlayerGun gun)
    {
        Transform muzzle = gun.GetMuzzlePoint();

        Vector3 origin = muzzle != null
            ? muzzle.position
            : transform.position + transform.forward * 0.5f + Vector3.up * 0.5f;

        Vector3 direction = transform.forward;

        if (Physics.Raycast(origin, direction, out RaycastHit hit, shootRange, shootMask))
        {
            DamageEnemy(hit, gun.GetDamage());
            SpawnBulletHole(hit);
        }
    }

    private void DamageEnemy(RaycastHit hit, float damage)
    {
        EnemyController meleeEnemy = hit.collider.GetComponentInParent<EnemyController>();
        EnemyRangedController rangedEnemy = hit.collider.GetComponentInParent<EnemyRangedController>();

        if (meleeEnemy != null)
            meleeEnemy.enemyhit(damage);
        else if (rangedEnemy != null)
            rangedEnemy.enemyhit(damage);
    }

    private void SpawnBulletHole(RaycastHit hit)
    {
        if (bulletHolePrefab == null)
            return;

        int groundLayer = LayerMask.NameToLayer("Ground");

        if (hit.collider.gameObject.layer != groundLayer)
            return;

        Vector3 holePos = hit.point + hit.normal * 0.01f;

        GameObject hole = Instantiate(
            bulletHolePrefab,
            holePos,
            Quaternion.LookRotation(hit.normal)
        );

        Destroy(hole, bulletHoleLifetime);
    }

    private void SpawnMuzzleFlash(PlayerGun gun)
    {
        GameObject flashPrefab = gun.GetMuzzleFlash();

        if (flashPrefab == null)
            return;

        Transform muzzle = gun.GetMuzzlePoint();

        Vector3 position = muzzle != null
            ? muzzle.position
            : transform.position + transform.forward * 0.5f;

        Quaternion rotation = muzzle != null
            ? muzzle.rotation
            : transform.rotation;

        GameObject flash = Instantiate(flashPrefab, position, rotation);
        Destroy(flash, muzzleFlashLifetime);
    }

    private IEnumerator ShootCooldown(float fireRate)
    {
        canShoot = false;
        yield return new WaitForSeconds(fireRate);
        canShoot = true;
    }

    public void EquipPrimary()
    {
        usingPrimary = true;

        if (primaryGun != null)
            primaryGun.SetGunActive(true);

        if (secondaryGun != null)
            secondaryGun.SetGunActive(false);

        OnWeaponChanged?.Invoke(primaryGun);
        OnAmmoCheckRequested?.Invoke(primaryGun);
    }

    public void EquipSecondary()
    {
        usingPrimary = false;

        if (primaryGun != null)
            primaryGun.SetGunActive(false);

        if (secondaryGun != null)
            secondaryGun.SetGunActive(true);

        OnWeaponChanged?.Invoke(secondaryGun);
        OnAmmoCheckRequested?.Invoke(secondaryGun);
    }

    public void SetSprinting(bool sprinting)
    {
        isSprinting = sprinting;

        PlayerGun gun = GetCurrentGun();

        if (gun != null)
            gun.SetSprinting(sprinting);
    }

    public void SetHasAmmo(bool value)
    {
        hasAmmo = value;
    }

    public void AddAmmoToCurrentWeapon()
    {
        OnWeaponChanged?.Invoke(GetCurrentGun());
    }

    public bool NeedsAmmo()
    {
        return true;
    }

    public void SwapCurrentWeapon(GameObject touchedPickupObject, GameObject newHeldGunPrefab)
    {
        if (newHeldGunPrefab == null || gunHoldParent == null)
            return;

        Vector3 dropPos = transform.position + transform.forward * 1.5f + Vector3.up * 0.5f;

        if (usingPrimary)
            SwapPrimaryWeapon(touchedPickupObject, newHeldGunPrefab, dropPos);
        else
            SwapSecondaryWeapon(touchedPickupObject, newHeldGunPrefab, dropPos);
    }

    private void SwapPrimaryWeapon(GameObject touchedPickupObject, GameObject newHeldGunPrefab, Vector3 dropPos)
    {
        PlayerGun oldGun = primaryGun;

        if (touchedPickupObject != null)
            Destroy(touchedPickupObject);

        DropOldWeapon(oldGun, dropPos);

        if (oldGun != null)
            Destroy(oldGun.gameObject);

        GameObject newGunObj = Instantiate(newHeldGunPrefab, gunHoldParent);
        newGunObj.transform.localPosition = Vector3.zero;
        newGunObj.transform.localRotation = Quaternion.identity;

        primaryGun = newGunObj.GetComponent<PlayerGun>();
        InitializeGun(primaryGun);
        EquipPrimary();
    }

    private void SwapSecondaryWeapon(GameObject touchedPickupObject, GameObject newHeldGunPrefab, Vector3 dropPos)
    {
        PlayerGun oldGun = secondaryGun;

        if (touchedPickupObject != null)
            Destroy(touchedPickupObject);

        DropOldWeapon(oldGun, dropPos);

        if (oldGun != null)
            Destroy(oldGun.gameObject);

        GameObject newGunObj = Instantiate(newHeldGunPrefab, gunHoldParent);
        newGunObj.transform.localPosition = Vector3.zero;
        newGunObj.transform.localRotation = Quaternion.identity;

        secondaryGun = newGunObj.GetComponent<PlayerGun>();
        InitializeGun(secondaryGun);
        EquipSecondary();
    }

    private void DropOldWeapon(PlayerGun oldGun, Vector3 dropPos)
    {
        if (oldGun == null)
            return;

        GameObject pickupPrefab = oldGun.GetWorldPickupPrefab();

        if (pickupPrefab == null)
            return;

        GameObject dropped = Instantiate(pickupPrefab, dropPos, Quaternion.identity);

        WeaponPickup pickup = dropped.GetComponent<WeaponPickup>();

        if (pickup != null)
            pickup.SetPickupCooldownPlayer(transform);
    }

    public PlayerGun GetCurrentGun()
    {
        return usingPrimary ? primaryGun : secondaryGun;
    }

    public void SetAttackSpeedMultiplier(float multiplier)
    {
        attackSpeedMultiplier = multiplier;
    }
}