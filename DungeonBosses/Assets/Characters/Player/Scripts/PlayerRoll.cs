using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class PlayerRoll : MonoBehaviour
{
    [Header("Roll")]
    [SerializeField] private float rollDistance = 15f;
    [SerializeField] private float rollDuration = 0.25f;
    [SerializeField] private float rollCooldown = 1f;
    [SerializeField] private float invulnerableTime = 0.4f;

    private CharacterController controller;
    private PlayerHealth playerHealth;

    private bool canRoll = true;
    private bool isRolling;

    public bool IsRolling => isRolling;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        playerHealth = GetComponent<PlayerHealth>();
    }

    public void Roll(Vector2 moveInput)
    {
        if (!canRoll || isRolling)
            return;

        Vector3 rollDirection;

        if (moveInput.sqrMagnitude > 0.01f)
        {
            rollDirection = new Vector3(moveInput.x, 0f, moveInput.y);
        }
        else
        {
            rollDirection = transform.forward;
        }

        rollDirection.y = 0f;
        rollDirection.Normalize();

        StartCoroutine(RollRoutine(rollDirection));
    }

    private IEnumerator RollRoutine(Vector3 rollDirection)
    {
        canRoll = false;
        isRolling = true;

        if (playerHealth != null)
            playerHealth.SetInvulnerable(true);

        float elapsed = 0f;
        float rollSpeed = rollDistance / rollDuration;

        while (elapsed < rollDuration)
        {
            controller.Move(rollDirection * rollSpeed * Time.deltaTime);

            elapsed += Time.deltaTime;
            yield return null;
        }

        isRolling = false;

        yield return new WaitForSeconds(Mathf.Max(0f, invulnerableTime - rollDuration));

        if (playerHealth != null)
            playerHealth.SetInvulnerable(false);

        yield return new WaitForSeconds(rollCooldown);

        canRoll = true;
    }
}