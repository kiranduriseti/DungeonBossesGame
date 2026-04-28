using UnityEngine;
using System.Collections;

public class PlayerSkill : MonoBehaviour
{
    [Header("Skill Buff")]
    [SerializeField] private float duration = 4f;
    [SerializeField] private float cooldown = 8f;
    [SerializeField] private float movementSpeedMultiplier = 1.5f;
    [SerializeField] private float attackSpeedMultiplier = 1.5f;

    private PlayerMovement playerMovement;
    private PlayerWeaponHandler weaponHandler;

    private bool canUseSkill = true;

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        weaponHandler = GetComponent<PlayerWeaponHandler>();
    }

    public void ActivateSkill()
    {
        if (!canUseSkill)
            return;

        StartCoroutine(SkillRoutine());
    }

    private IEnumerator SkillRoutine()
    {
        canUseSkill = false;

        if (playerMovement != null)
            playerMovement.SetSpeedMultiplier(movementSpeedMultiplier);

        if (weaponHandler != null)
            weaponHandler.SetAttackSpeedMultiplier(attackSpeedMultiplier);

        yield return new WaitForSeconds(duration);

        if (playerMovement != null)
            playerMovement.SetSpeedMultiplier(1f);

        if (weaponHandler != null)
            weaponHandler.SetAttackSpeedMultiplier(1f);

        yield return new WaitForSeconds(cooldown);

        canUseSkill = true;
    }
}