// using UnityEngine;
// using UnityEngine.Events;
// using System.Collections.Generic;

// public class PlayerAmmo : MonoBehaviour
// {
//     [Header("Invoke Events")]
//     public UnityEvent<float, float> OnAmmoChanged;
//     public UnityEvent OnAmmoEmpty;

//     private Dictionary<string, float> currentAmmo = new Dictionary<string, float>();
//     private Dictionary<string, float> maxAmmo = new Dictionary<string, float>();

//     public void RegisterWeapon(PlayerGun gun)
//     {
//         if (gun == null)
//             return;

//         string gunName = gun.GetGunName();

//         if (!currentAmmo.ContainsKey(gunName))
//         {
//             currentAmmo[gunName] = gun.GetStartingAmmo();
//             maxAmmo[gunName] = gun.GetStartingAmmo();
//         }
//     }

//     public bool HasAmmo(PlayerGun gun)
//     {
//         if (gun == null)
//             return false;

//         RegisterWeapon(gun);

//         return currentAmmo[gun.GetGunName()] > 0f;
//     }

//     public void SpendAmmo(PlayerGun gun)
//     {
//         if (gun == null)
//             return;

//         RegisterWeapon(gun);

//         string gunName = gun.GetGunName();

//         if (currentAmmo[gunName] <= 0f)
//         {
//             OnAmmoEmpty?.Invoke();
//             return;
//         }

//         currentAmmo[gunName]--;

//         InvokeAmmoChanged(gun);
//     }

//     public void AddAmmo(PlayerGun gun)
//     {
//         if (gun == null)
//             return;

//         RegisterWeapon(gun);

//         string gunName = gun.GetGunName();
//         float amount = gun.GetPickupAmmoAmount();

//         currentAmmo[gunName] += amount;
//         currentAmmo[gunName] = Mathf.Min(currentAmmo[gunName], maxAmmo[gunName]);

//         InvokeAmmoChanged(gun);
//     }

//     public bool NeedsAmmo(PlayerGun gun)
//     {
//         if (gun == null)
//             return false;

//         RegisterWeapon(gun);

//         string gunName = gun.GetGunName();

//         return currentAmmo[gunName] < maxAmmo[gunName];
//     }

//     public void InvokeAmmoChanged(PlayerGun gun)
//     {
//         if (gun == null)
//             return;

//         RegisterWeapon(gun);

//         string gunName = gun.GetGunName();

//         OnAmmoChanged?.Invoke(maxAmmo[gunName], currentAmmo[gunName]);
//     }

//     public float GetCurrentAmmo(PlayerGun gun)
//     {
//         if (gun == null)
//             return 0f;

//         RegisterWeapon(gun);

//         return currentAmmo[gun.GetGunName()];
//     }

//     public float GetMaxAmmo(PlayerGun gun)
//     {
//         if (gun == null)
//             return 0f;

//         RegisterWeapon(gun);

//         return maxAmmo[gun.GetGunName()];
//     }
// }

using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class PlayerAmmo : MonoBehaviour
{
    [Header("Events")]
    public UnityEvent<float, float> OnAmmoChanged;
    public UnityEvent OnAmmoEmpty;

    private readonly Dictionary<string, float> currentAmmo = new Dictionary<string, float>();
    private readonly Dictionary<string, float> maxAmmo = new Dictionary<string, float>();

    public void RegisterWeapon(PlayerGun gun)
    {
        if (gun == null)
            return;

        string gunName = gun.GetGunName();

        if (currentAmmo.ContainsKey(gunName))
            return;

        currentAmmo[gunName] = gun.GetStartingAmmo();
        maxAmmo[gunName] = gun.GetMaxAmmo();
    }

    public bool HasAmmo(PlayerGun gun)
    {
        if (gun == null)
            return false;

        RegisterWeapon(gun);
        return currentAmmo[gun.GetGunName()] > 0f;
    }

    public void SpendAmmo(PlayerGun gun)
    {
        if (gun == null)
            return;

        RegisterWeapon(gun);

        string gunName = gun.GetGunName();

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

        string gunName = gun.GetGunName();

        currentAmmo[gunName] += gun.GetPickupAmmoAmount();
        currentAmmo[gunName] = Mathf.Min(currentAmmo[gunName], maxAmmo[gunName]);

        InvokeAmmoChanged(gun);
    }

    public bool NeedsAmmo(PlayerGun gun)
    {
        if (gun == null)
            return false;

        RegisterWeapon(gun);

        string gunName = gun.GetGunName();
        return currentAmmo[gunName] < maxAmmo[gunName];
    }

    public void InvokeAmmoChanged(PlayerGun gun)
    {
        if (gun == null)
            return;

        RegisterWeapon(gun);

        string gunName = gun.GetGunName();
        OnAmmoChanged?.Invoke(maxAmmo[gunName], currentAmmo[gunName]);
    }
}