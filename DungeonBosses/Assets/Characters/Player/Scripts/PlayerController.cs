using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    [Header("Combat")]
    [SerializeField] private float shootRange = 100f;
    [SerializeField] private LayerMask shootMask = ~0;

    [Header("References")]
    [SerializeField] private PlayerAnim anim;
    [SerializeField] private PlayerGun primaryGun;
    [SerializeField] private PlayerGun secondaryGun;
    [SerializeField] private Transform gunHoldParent;

    [Header("Effects")]
    [SerializeField] private GameObject deathEffect;
    [SerializeField] private GameObject bulletHolePrefab;
    [SerializeField] private float bulletHoleLifetime = 5f;
    [SerializeField] private float flashTime = 0.1f;

    [Header("Health")]
    [SerializeField] private float health = 100f;
    private float maxHealth;

    private MovementControls controls;

    private bool usingPrimary = true;
    private bool isSprinting;
    private bool canShoot = true;

    private float primaryDamage;
    private float secondaryDamage;

    private float primaryFireRate;
    private float secondaryFireRate;

    private float primaryAmmo;
    private float secondaryAmmo;

    private float primaryMaxAmmo;
    private float secondaryMaxAmmo;

    private Transform primaryMuzzlePoint;
    private Transform secondaryMuzzlePoint;

    private GameObject primaryMuzzleFlash;
    private GameObject secondaryMuzzleFlash;

    private readonly Dictionary<string, float> savedAmmo = new Dictionary<string, float>();
    private readonly Dictionary<string, float> savedMaxAmmo = new Dictionary<string, float>();

    public event System.Action<float, float> OnHealthChanged;

    private void Awake()
    {
        controls = new MovementControls();

        if (anim == null)
            anim = GetComponentInChildren<PlayerAnim>();
    }

    private void OnEnable()
    {
        controls.Enable();
        controls.PlayerMap.Cast.performed += OnCast;
    }

    private void OnDisable()
    {
        controls.PlayerMap.Cast.performed -= OnCast;
        controls.Disable();
    }

    private void Start()
    {
        maxHealth = health;

        InitializeWeapons();

        EquipPrimary();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.setMaxHealth(maxHealth);
            GameManager.Instance.updateHealth(health);
            GameManager.Instance.setMaxAmmo(primaryMaxAmmo);
            GameManager.Instance.updateAmmo(primaryAmmo);
        }

        OnHealthChanged?.Invoke(maxHealth, health);
    }

    private void InitializeWeapons()
    {
        if (primaryGun != null)
        {
            primaryGun.InitializeWeapon();

            primaryDamage = primaryGun.GetDamage();
            primaryFireRate = primaryGun.GetFireRate();
            primaryAmmo = primaryGun.GetStartingAmmo();
            primaryMaxAmmo = primaryAmmo;
            primaryMuzzlePoint = primaryGun.GetMuzzlePoint();
            primaryMuzzleFlash = primaryGun.GetMuzzleFlash();

            SaveWeaponAmmo(primaryGun, primaryAmmo, primaryMaxAmmo);
        }

        if (secondaryGun != null)
        {
            secondaryGun.InitializeWeapon();

            secondaryDamage = secondaryGun.GetDamage();
            secondaryFireRate = secondaryGun.GetFireRate();
            secondaryAmmo = secondaryGun.GetStartingAmmo();
            secondaryMaxAmmo = secondaryAmmo;
            secondaryMuzzlePoint = secondaryGun.GetMuzzlePoint();
            secondaryMuzzleFlash = secondaryGun.GetMuzzleFlash();

            SaveWeaponAmmo(secondaryGun, secondaryAmmo, secondaryMaxAmmo);
        }
    }

    private void OnCast(InputAction.CallbackContext context)
    {
        FireWeapon();
    }

    public void FireWeapon()
    {
        if (!canShoot || isSprinting || health <= 0f)
            return;

        PlayerGun gun = GetCurrentGun();

        if (gun == null)
            return;

        if (GetCurrentAmmo() <= 0f)
        {
            if (GameManager.Instance != null)
                GameManager.Instance.AmmoWarning();

            return;
        }

        SpawnMuzzleFlash();

        Vector3 origin = GetShootOrigin();
        Vector3 direction = transform.forward;

        if (Physics.Raycast(origin, direction, out RaycastHit hit, shootRange, shootMask))
        {
            DamageEnemy(hit);
            SpawnBulletHole(hit);
        }

        if (anim != null)
            anim.SetTrigger("Cast");

        gun.Fire();
        SpendAmmo();

        StartCoroutine(ShootCooldown(GetCurrentFireRate()));
    }

    private Vector3 GetShootOrigin()
    {
        Transform muzzle = GetCurrentMuzzlePoint();

        if (muzzle != null)
            return muzzle.position;

        return transform.position + Vector3.up * 0.5f + transform.forward * 0.5f;
    }

    private void DamageEnemy(RaycastHit hit)
    {
        float damage = GetCurrentDamage();

        // EnemyController meleeEnemy = hit.collider.GetComponentInParent<EnemyController>();
        // EnemyRangedController rangedEnemy = hit.collider.GetComponentInParent<EnemyRangedController>();

        // if (meleeEnemy != null)
        // {
        //     meleeEnemy.enemyhit(damage);
        // }
        // else if (rangedEnemy != null)
        // {
        //     rangedEnemy.enemyhit(damage);
        // }

        BossController boss = hit.collider.GetComponentInParent<BossController>();
        boss.enemyhit(damage);
        
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

    private void SpawnMuzzleFlash()
    {
        GameObject flashPrefab = GetCurrentMuzzleFlash();

        if (flashPrefab == null)
            return;

        Transform muzzle = GetCurrentMuzzlePoint();

        Vector3 flashPos = muzzle != null ? muzzle.position : GetShootOrigin();
        Quaternion flashRot = muzzle != null ? muzzle.rotation : transform.rotation;

        GameObject flash = Instantiate(flashPrefab, flashPos, flashRot);
        Destroy(flash, flashTime);
    }

    private IEnumerator ShootCooldown(float fireRate)
    {
        canShoot = false;
        yield return new WaitForSeconds(fireRate);
        canShoot = true;
    }

    public void SwitchToPrimary()
    {
        if (usingPrimary)
            return;

        EquipPrimary();
    }

    public void SwitchToSecondary()
    {
        if (!usingPrimary)
            return;

        EquipSecondary();
    }

    public void switchWeapon(float pos)
    {
        if (pos == 1)
            SwitchToPrimary();
        else if (pos == 2)
            SwitchToSecondary();
    }

    private void EquipPrimary()
    {
        usingPrimary = true;

        if (primaryGun != null)
            primaryGun.SetGunActive(true);

        if (secondaryGun != null)
            secondaryGun.SetGunActive(false);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.setMaxAmmo(primaryMaxAmmo);
            GameManager.Instance.updateAmmo(primaryAmmo);
        }
    }

    private void EquipSecondary()
    {
        usingPrimary = false;

        if (primaryGun != null)
            primaryGun.SetGunActive(false);

        if (secondaryGun != null)
            secondaryGun.SetGunActive(true);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.setMaxAmmo(secondaryMaxAmmo);
            GameManager.Instance.updateAmmo(secondaryAmmo);
        }
    }

    public void sprintAnim(bool sprinting)
    {
        isSprinting = sprinting;

        PlayerGun gun = GetCurrentGun();

        if (gun != null)
            gun.SetSprinting(sprinting);
    }

    public void PlayerHit(float damage = 10f)
    {
        if (health <= 0f)
            return;

        health -= damage;
        health = Mathf.Max(health, 0f);

        OnHealthChanged?.Invoke(maxHealth, health);

        if (GameManager.Instance != null)
            GameManager.Instance.updateHealth(health);

        if (health <= 0f)
            Die();
    }

    public void playerhit(float damage = 10f)
    {
        PlayerHit(damage);
    }

    private void Die()
    {
        if (anim != null)
            anim.SetPlayerDead(true);

        if (deathEffect != null)
            Instantiate(deathEffect, transform.position, Quaternion.identity);

        Invoke(nameof(DelayedLoseGame), 0.5f);
    }

    private void DelayedLoseGame()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.LoseGame();
    }

    public void Heal(float amount)
    {
        if (health <= 0f)
            return;

        health += amount;
        health = Mathf.Min(health, maxHealth);

        OnHealthChanged?.Invoke(maxHealth, health);

        if (GameManager.Instance != null)
            GameManager.Instance.updateHealth(health);
    }

    public bool NeedsHealth()
    {
        return health < maxHealth;
    }

    public void AddAmmoToCurrentWeapon()
    {
        PlayerGun gun = GetCurrentGun();

        if (gun == null)
            return;

        float amount = gun.GetPickupAmmoAmount();

        if (usingPrimary)
        {
            primaryAmmo += amount;
            primaryAmmo = Mathf.Min(primaryAmmo, primaryMaxAmmo);

            SaveWeaponAmmo(primaryGun, primaryAmmo, primaryMaxAmmo);

            if (GameManager.Instance != null)
                GameManager.Instance.updateAmmo(primaryAmmo);
        }
        else
        {
            secondaryAmmo += amount;
            secondaryAmmo = Mathf.Min(secondaryAmmo, secondaryMaxAmmo);

            SaveWeaponAmmo(secondaryGun, secondaryAmmo, secondaryMaxAmmo);

            if (GameManager.Instance != null)
                GameManager.Instance.updateAmmo(secondaryAmmo);
        }
    }

    public bool NeedsAmmo()
    {
        return GetCurrentAmmo() < GetCurrentMaxAmmo();
    }

    // public void SwapCurrentWeapon(GameObject touchedPickupObject, GameObject newHeldGunPrefab)
    // {
    //     if (newHeldGunPrefab == null || gunHoldParent == null)
    //         return;

    //     Vector3 dropPos = transform.position + transform.forward * 1.5f + Vector3.up * 0.5f;

    //     if (usingPrimary)
    //         SwapPrimaryWeapon(touchedPickupObject, newHeldGunPrefab, dropPos);
    //     else
    //         SwapSecondaryWeapon(touchedPickupObject, newHeldGunPrefab, dropPos);
    // }

    // private void SwapPrimaryWeapon(GameObject touchedPickupObject, GameObject newHeldGunPrefab, Vector3 dropPos)
    // {
    //     PlayerGun oldGun = primaryGun;

    //     if (touchedPickupObject != null)
    //         Destroy(touchedPickupObject);

    //     if (oldGun != null)
    //     {
    //         SaveWeaponAmmo(oldGun, primaryAmmo, primaryMaxAmmo);
    //         DropOldWeapon(oldGun, dropPos);
    //         Destroy(oldGun.gameObject);
    //     }

    //     GameObject newGunObj = Instantiate(newHeldGunPrefab, gunHoldParent);
    //     newGunObj.transform.localPosition = Vector3.zero;
    //     newGunObj.transform.localRotation = Quaternion.identity;

    //     primaryGun = newGunObj.GetComponent<PlayerGun>();
    //     primaryGun.InitializeWeapon();

    //     primaryDamage = primaryGun.GetDamage();
    //     primaryFireRate = primaryGun.GetFireRate();

    //     LoadWeaponAmmo(primaryGun, primaryGun.GetStartingAmmo(), out primaryAmmo, out primaryMaxAmmo);

    //     primaryMuzzlePoint = primaryGun.GetMuzzlePoint();
    //     primaryMuzzleFlash = primaryGun.GetMuzzleFlash();

    //     EquipPrimary();
    // }

    // private void SwapSecondaryWeapon(GameObject touchedPickupObject, GameObject newHeldGunPrefab, Vector3 dropPos)
    // {
    //     PlayerGun oldGun = secondaryGun;

    //     if (touchedPickupObject != null)
    //         Destroy(touchedPickupObject);

    //     if (oldGun != null)
    //     {
    //         SaveWeaponAmmo(oldGun, secondaryAmmo, secondaryMaxAmmo);
    //         DropOldWeapon(oldGun, dropPos);
    //         Destroy(oldGun.gameObject);
    //     }

    //     GameObject newGunObj = Instantiate(newHeldGunPrefab, gunHoldParent);
    //     newGunObj.transform.localPosition = Vector3.zero;
    //     newGunObj.transform.localRotation = Quaternion.identity;

    //     secondaryGun = newGunObj.GetComponent<PlayerGun>();
    //     secondaryGun.InitializeWeapon();

    //     secondaryDamage = secondaryGun.GetDamage();
    //     secondaryFireRate = secondaryGun.GetFireRate();

    //     LoadWeaponAmmo(secondaryGun, secondaryGun.GetAmmo(), out secondaryAmmo, out secondaryMaxAmmo);

    //     secondaryMuzzlePoint = secondaryGun.GetMuzzlePoint();
    //     secondaryMuzzleFlash = secondaryGun.GetMuzzleFlash();

    //     EquipSecondary();
    // }

    // private void DropOldWeapon(PlayerGun oldGun, Vector3 dropPos)
    // {
    //     GameObject oldPickupPrefab = oldGun.GetWorldPickupPrefab();

    //     if (oldPickupPrefab == null)
    //         return;

    //     GameObject dropped = Instantiate(oldPickupPrefab, dropPos, Quaternion.identity);

    //     WeaponPickup droppedPickup = dropped.GetComponent<WeaponPickup>();

    //     if (droppedPickup != null)
    //         droppedPickup.SetPickupCooldownPlayer(transform);
    // }

    private PlayerGun GetCurrentGun()
    {
        return usingPrimary ? primaryGun : secondaryGun;
    }

    private float GetCurrentDamage()
    {
        return usingPrimary ? primaryDamage : secondaryDamage;
    }

    private float GetCurrentFireRate()
    {
        return usingPrimary ? primaryFireRate : secondaryFireRate;
    }

    private float GetCurrentAmmo()
    {
        return usingPrimary ? primaryAmmo : secondaryAmmo;
    }

    private float GetCurrentMaxAmmo()
    {
        return usingPrimary ? primaryMaxAmmo : secondaryMaxAmmo;
    }

    private Transform GetCurrentMuzzlePoint()
    {
        return usingPrimary ? primaryMuzzlePoint : secondaryMuzzlePoint;
    }

    private GameObject GetCurrentMuzzleFlash()
    {
        return usingPrimary ? primaryMuzzleFlash : secondaryMuzzleFlash;
    }

    private void SpendAmmo()
    {
        if (usingPrimary)
        {
            primaryAmmo--;
            SaveWeaponAmmo(primaryGun, primaryAmmo, primaryMaxAmmo);

            if (GameManager.Instance != null)
                GameManager.Instance.updateAmmo(primaryAmmo);
        }
        else
        {
            secondaryAmmo--;
            SaveWeaponAmmo(secondaryGun, secondaryAmmo, secondaryMaxAmmo);

            if (GameManager.Instance != null)
                GameManager.Instance.updateAmmo(secondaryAmmo);
        }
    }

    private void SaveWeaponAmmo(PlayerGun gun, float currentAmmoValue, float maxAmmoValue)
    {
        if (gun == null)
            return;

        string gunName = gun.GetGunName();

        savedAmmo[gunName] = currentAmmoValue;
        savedMaxAmmo[gunName] = maxAmmoValue;
    }

    private void LoadWeaponAmmo(PlayerGun gun, float defaultAmmo, out float currentAmmoValue, out float maxAmmoValue)
    {
        currentAmmoValue = defaultAmmo;
        maxAmmoValue = defaultAmmo;

        if (gun == null)
            return;

        string gunName = gun.GetGunName();

        if (savedAmmo.TryGetValue(gunName, out float storedAmmo))
            currentAmmoValue = storedAmmo;

        if (savedMaxAmmo.TryGetValue(gunName, out float storedMaxAmmo))
            maxAmmoValue = storedMaxAmmo;
    }
}