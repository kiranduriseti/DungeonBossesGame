using UnityEngine;

public class PlayerGun : MonoBehaviour
{
    [Header("Name")]
    [SerializeField] private string gunName;

    [Header("References")]
    [SerializeField] private GameObject muzzleFlash;
    [SerializeField] private Transform muzzlePoint;
    [SerializeField] private GameObject worldPickupPrefab;

    [Header("Stats")]
    [SerializeField] private float fireRate = 0.75f;
    [SerializeField] private float damage = 20f;
    [SerializeField] private float ammo = 20f;
    [SerializeField] private float pickupAmmo = 2f;

    [Header("Recoil")]
    [SerializeField] private float recoilAngle = 20f;
    [SerializeField] private float recoilSpeed = 20f;
    [SerializeField] private float returnSpeed = 12f;
    [SerializeField] private float recoilDirection = -1f;

    [Header("Sprint Pose")]
    [SerializeField] private Vector3 sprintLocalPositionOffset = new Vector3(0f, 0f, -0.1f);
    [SerializeField] private Vector3 sprintLocalRotationOffset = new Vector3(0f, 0f, 15f);
    [SerializeField] private float poseLerpSpeed = 10f;

    private Vector3 normalLocalPosition;
    private Quaternion normalLocalRotation;

    private Vector3 currentBasePosition;
    private Quaternion currentBaseRotation;

    private Quaternion recoilOffset = Quaternion.identity;
    private Quaternion targetRecoilOffset = Quaternion.identity;

    private bool isSprinting;

    private void Awake()
    {
        InitializeWeapon();
    }

    private void LateUpdate()
    {
        Vector3 targetPosition = isSprinting
            ? normalLocalPosition + sprintLocalPositionOffset
            : normalLocalPosition;

        Quaternion sprintOffset = isSprinting
            ? Quaternion.Euler(sprintLocalRotationOffset)
            : Quaternion.identity;

        Quaternion targetRotation = normalLocalRotation * sprintOffset;

        currentBasePosition = Vector3.Lerp(
            currentBasePosition,
            targetPosition,
            poseLerpSpeed * Time.deltaTime
        );

        currentBaseRotation = Quaternion.Slerp(
            currentBaseRotation,
            targetRotation,
            poseLerpSpeed * Time.deltaTime
        );

        targetRecoilOffset = Quaternion.Slerp(
            targetRecoilOffset,
            Quaternion.identity,
            returnSpeed * Time.deltaTime
        );

        recoilOffset = Quaternion.Slerp(
            recoilOffset,
            targetRecoilOffset,
            recoilSpeed * Time.deltaTime
        );

        transform.localPosition = currentBasePosition;
        transform.localRotation = currentBaseRotation * recoilOffset;
    }

    public void InitializeWeapon()
    {
        normalLocalPosition = transform.localPosition;
        normalLocalRotation = transform.localRotation;

        currentBasePosition = normalLocalPosition;
        currentBaseRotation = normalLocalRotation;

        recoilOffset = Quaternion.identity;
        targetRecoilOffset = Quaternion.identity;
    }

    public void Fire()
    {
        targetRecoilOffset *= Quaternion.Euler(0f, recoilDirection * recoilAngle, 0f);
    }

    public void SetGunActive(bool active)
    {
        gameObject.SetActive(active);
    }

    public void SetSprinting(bool sprinting)
    {
        isSprinting = sprinting;
    }

    public string GetGunName()
    {
        return gunName;
    }

    public float GetDamage()
    {
        return damage;
    }

    public float GetFireRate()
    {
        return fireRate;
    }

    public float GetStartingAmmo()
    {
        return ammo;
    }

    public float GetPickupAmmoAmount()
    {
        return pickupAmmo;
    }

    public GameObject GetMuzzleFlash()
    {
        return muzzleFlash;
    }

    public Transform GetMuzzlePoint()
    {
        return muzzlePoint;
    }

    public GameObject GetWorldPickupPrefab()
    {
        return worldPickupPrefab;
    }
}