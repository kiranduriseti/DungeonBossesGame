using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform playerBody;

    [Header("Top Down Camera")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 12f, -2f);
    [SerializeField] private float followSmoothSpeed = 10f;
    [SerializeField] private bool lookAtPlayer = true;

    [Header("Camera Bob")]
    [SerializeField] private bool useCameraBob = true;
    [SerializeField] private float walkBobSpeed = 10f;
    [SerializeField] private float walkBobAmount = 0.05f;
    [SerializeField] private float sprintBobSpeed = 16f;
    [SerializeField] private float sprintBobAmount = 0.1f;

    private PlayerRoll playerRoll;

    private float bobTimer;
    private bool isMoving;
    private bool isSprinting;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (playerBody != null)
            playerRoll = playerBody.GetComponent<PlayerRoll>();
    }

    // private void LateUpdate()
    // {
    //     if (playerBody == null)
    //         return;

    //     FollowPlayer();

    //     if (lookAtPlayer)
    //         LookAtPlayer();
    // }
    private void LateUpdate()
    {
        if (playerBody == null)
            return;

        FollowPlayer();

        transform.rotation = Quaternion.Euler(55f, 0f, 0f);
    }

    private void FollowPlayer()
    {
        bool isRolling = playerRoll != null && playerRoll.IsRolling;

        Vector3 bobOffset = useCameraBob && !isRolling
            ? GetBobOffset()
            : Vector3.zero;

        if (isRolling)
            bobTimer = 0f;

        Vector3 targetPosition = playerBody.position + offset + bobOffset;

        if (isRolling)
        {
            transform.position = targetPosition;
        }
        else
        {
            transform.position = Vector3.Lerp(
                transform.position,
                targetPosition,
                followSmoothSpeed * Time.deltaTime
            );
        }
    }

    private void LookAtPlayer()
    {
        transform.LookAt(playerBody.position);
    }

    private Vector3 GetBobOffset()
    {
        if (!isMoving)
        {
            bobTimer = 0f;
            return Vector3.zero;
        }

        float bobSpeed = isSprinting ? sprintBobSpeed : walkBobSpeed;
        float bobAmount = isSprinting ? sprintBobAmount : walkBobAmount;

        bobTimer += Time.deltaTime * bobSpeed;

        float xOffset = Mathf.Cos(bobTimer * 0.5f) * bobAmount;
        float yOffset = Mathf.Sin(bobTimer) * bobAmount;

        return new Vector3(xOffset, yOffset, 0f);
    }

    public void SetMovementState(bool moving, bool sprinting)
    {
        isMoving = moving;
        isSprinting = sprinting;
    }

    public void SetPlayerBody(Transform newPlayerBody)
    {
        playerBody = newPlayerBody;
        playerRoll = playerBody != null ? playerBody.GetComponent<PlayerRoll>() : null;
    }
}