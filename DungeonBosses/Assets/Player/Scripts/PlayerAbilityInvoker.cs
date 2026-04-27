using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAbilityInvoker : MonoBehaviour
{
    private MovementControls controls;

    private PlayerRoll playerRoll;
    private PlayerSkill speedBuff;

    private void Awake()
    {
        controls = new MovementControls();

        playerRoll = GetComponent<PlayerRoll>();
        speedBuff = GetComponent<PlayerSkillSpeedBuff>();
    }

    private void OnEnable()
    {
        controls.Enable();

        controls.PlayerMap.Roll.performed += OnRoll;
        controls.PlayerMap.Skill.performed += OnSkill;
    }

    private void OnDisable()
    {
        controls.PlayerMap.Roll.performed -= OnRoll;
        controls.PlayerMap.Skill.performed -= OnSkill;

        controls.Disable();
    }

    private void OnRoll(InputAction.CallbackContext context)
    {
        if (playerRoll != null)
            playerRoll.Roll();
    }

    private void OnSkill(InputAction.CallbackContext context)
    {
        if (speedBuff != null)
            speedBuff.ActivateSkill();
    }
}