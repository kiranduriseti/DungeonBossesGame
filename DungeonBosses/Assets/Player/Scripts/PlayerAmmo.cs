using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class PlayerAmmo : MonoBehaviour
{
    [Header("Invoke Events")]
    public UnityEvent<float, float> OnAmmoChanged;
    public UnityEvent OnAmmoEmpty;

    private Dictionary<string, float> currentAmmo = new Dictionary<string, float>();
    private Dictionary<string, float> maxAmmo = new Dictionary<string, float>();

    public void RegisterWeapon(PlayerGun gun)
    {
        if (gun == null)
            return;

        string gunName = gun.getGunName();

        if (!currentAmmo.ContainsKey(gunName))
        {
            currentAmmo[gunName] = gun.getAmmo();
            maxAmmo[gunName] = gun.getAmmo();
        }
    }

    public bool HasAmmo(PlayerGun gun)
    {
        if (gun == null)
            return false;

        RegisterWeapon(gun);

        return currentAmmo[gun.getGunName()] > 0f;
    }

    public void SpendAmmo(PlayerGun gun)
    {
        if (gun == null)
            return;

        RegisterWeapon(gun);

        string gunName = gun.getGunName();

        if (currentAmmo[gunName] <= 0f)
        {
            OnAmmoEmpty?.Invoke();
            return;
        }

        currentAmmo[gunName]--;

        InvokeAmmoChanged(gun);
    }

    public void AddAmmo(PlayerGun gun)
    {
        if (gun == null)
            return;

        RegisterWeapon(gun);

        string gunName = gun.getGunName();
        float amount = gun.getPickupAmmoAmount();

        currentAmmo[gunName] += amount;
        currentAmmo[gunName] = Mathf.Min(currentAmmo[gunName], maxAmmo[gunName]);

        InvokeAmmoChanged(gun);
    }

    public bool NeedsAmmo(PlayerGun gun)
    {
        if (gun == null)
            return false;

        RegisterWeapon(gun);

        string gunName = gun.getGunName();

        return currentAmmo[gunName] < maxAmmo[gunName];
    }

    public void InvokeAmmoChanged(PlayerGun gun)
    {
        if (gun == null)
            return;

        RegisterWeapon(gun);

        string gunName = gun.getGunName();

        OnAmmoChanged?.Invoke(maxAmmo[gunName], currentAmmo[gunName]);
    }

    public float GetCurrentAmmo(PlayerGun gun)
    {
        if (gun == null)
            return 0f;

        RegisterWeapon(gun);

        return currentAmmo[gun.getGunName()];
    }

    public float GetMaxAmmo(PlayerGun gun)
    {
        if (gun == null)
            return 0f;

        RegisterWeapon(gun);

        return maxAmmo[gun.getGunName()];
    }
}