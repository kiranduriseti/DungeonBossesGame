using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class PlayerRoll : MonoBehaviour
{
    [Header("Roll")]
    [SerializeField] private float rollDistance = 4f;
    [SerializeField] private float rollCooldown = 1f;
    [SerializeField] private float invulnerableTime = 0.4f;

    private CharacterController controller;
    private PlayerHealth playerHealth;

    private bool canRoll = true;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        playerHealth = GetComponent<PlayerHealth>();
    }

    public void Roll()
    {
        if (!canRoll)
            return;

        Vector3 rollDirection = transform.forward;
        rollDirection.y = 0f;

        if (rollDirection.sqrMagnitude < 0.01f)
            rollDirection = Vector3.forward;

        rollDirection.Normalize();

        controller.enabled = false;
        transform.position += rollDirection * rollDistance;
        controller.enabled = true;

        StartCoroutine(RollRoutine());
    }

    private IEnumerator RollRoutine()
    {
        canRoll = false;

        if (playerHealth != null)
            playerHealth.SetInvulnerable(true);

        yield return new WaitForSeconds(invulnerableTime);

        if (playerHealth != null)
            playerHealth.SetInvulnerable(false);

        yield return new WaitForSeconds(rollCooldown);

        canRoll = true;
    }
}