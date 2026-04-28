// using UnityEngine;
// using UnityEngine.Events;
// using UnityEngine.InputSystem;

// [RequireComponent(typeof(CharacterController))]
// public class PlayerMovement : MonoBehaviour
// {
//     [Header("Movement")]
//     [SerializeField] private float moveSpeed = 8f;
//     [SerializeField] private float sprintSpeed = 12f;
//     [SerializeField] private bool allowSprint = true;

//     [Header("References")]
//     [SerializeField] private Camera mainCamera;

//     [Header("Events")]
//     public UnityEvent<bool> OnSprintStateChanged;

//     private CharacterController controller;
//     private MovementControls controls;

//     private PlayerRoll playerRoll;
//     private PlayerSkill speedBuff;

//     private Vector2 moveInput;
//     private bool sprintHeld;
//     private bool wasSprinting;
//     private float speedMultiplier = 1f;
//     public Vector2 MoveInput => moveInput;

//     private void Awake()
//     {
//         controller = GetComponent<CharacterController>();
//         controls = new MovementControls();

//         playerRoll = GetComponent<PlayerRoll>();
//         speedBuff = GetComponent<PlayerSkill>();

//         if (mainCamera == null)
//             mainCamera = Camera.main;
//     }

//     private void OnEnable()
//     {
//         controls.Enable();

//         controls.PlayerMap.Move.performed += OnMove;
//         controls.PlayerMap.Move.canceled += OnMove;

//         controls.PlayerMap.Sprint.performed += OnSprint;
//         controls.PlayerMap.Sprint.canceled += OnSprint;

//         controls.PlayerMap.Roll.performed += OnRoll;
//         controls.PlayerMap.Skill.performed += OnSkill;
//     }

//     private void OnDisable()
//     {
//         controls.PlayerMap.Move.performed -= OnMove;
//         controls.PlayerMap.Move.canceled -= OnMove;

//         controls.PlayerMap.Sprint.performed -= OnSprint;
//         controls.PlayerMap.Sprint.canceled -= OnSprint;

//         controls.PlayerMap.Roll.performed -= OnRoll;
//         controls.PlayerMap.Skill.performed -= OnSkill;

//         controls.Disable();
//     }

//     private void Update()
//     {
//         MovePlayer();
//         FaceMouse();
//     }

//     private void MovePlayer()
//     {
//         if (playerRoll != null && playerRoll.IsRolling)
//             return;
//         Vector3 move = new Vector3(moveInput.x, 0f, moveInput.y);
//         move = Vector3.ClampMagnitude(move, 1f);

//         bool isMoving = move.sqrMagnitude > 0.01f;
//         bool isSprinting = allowSprint && sprintHeld && isMoving;

//         float speed = (isSprinting ? sprintSpeed : moveSpeed) * speedMultiplier;

//         controller.Move(move * speed * Time.deltaTime);

//         if (isSprinting != wasSprinting)
//         {
//             wasSprinting = isSprinting;
//             OnSprintStateChanged?.Invoke(isSprinting);
//         }
//     }

//     private void FaceMouse()
//     {
//         if (mainCamera == null || Mouse.current == null)
//             return;

//         Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
//         Plane groundPlane = new Plane(Vector3.up, transform.position);

//         if (groundPlane.Raycast(ray, out float distance))
//         {
//             Vector3 mouseWorldPos = ray.GetPoint(distance);
//             Vector3 lookDir = mouseWorldPos - transform.position;
//             lookDir.y = 0f;

//             if (lookDir.sqrMagnitude > 0.01f)
//                 transform.rotation = Quaternion.LookRotation(lookDir);
//         }
//     }

//     private void OnMove(InputAction.CallbackContext context)
//     {
//         moveInput = context.ReadValue<Vector2>();
//     }

//     private void OnSprint(InputAction.CallbackContext context)
//     {
//         sprintHeld = context.ReadValueAsButton();
//     }

//     private void OnRoll(InputAction.CallbackContext context)
//     {
//         if (playerRoll != null)
//             playerRoll.Roll(moveInput);
//     }

//     private void OnSkill(InputAction.CallbackContext context)
//     {
//         if (speedBuff != null)
//             speedBuff.ActivateSkill();
//     }

//     public void SetSpeedMultiplier(float multiplier)
//     {
//         speedMultiplier = multiplier;
//     }
// }


using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float sprintSpeed = 12f;
    [SerializeField] private bool allowSprint = true;

    [Header("References")]
    [SerializeField] private Camera mainCamera;

    [Header("Events")]
    public UnityEvent<bool> OnSprintStateChanged;

    private CharacterController controller;
    private MovementControls controls;

    private PlayerRoll playerRoll;
    private PlayerSkill playerSkill;

    private Vector2 moveInput;
    private bool sprintHeld;
    private bool wasSprinting;
    private float speedMultiplier = 1f;

    public Vector2 MoveInput => moveInput;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        controls = new MovementControls();

        playerRoll = GetComponent<PlayerRoll>();
        playerSkill = GetComponent<PlayerSkill>();

        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    private void OnEnable()
    {
        controls.Enable();

        controls.PlayerMap.Move.performed += OnMove;
        controls.PlayerMap.Move.canceled += OnMove;

        controls.PlayerMap.Sprint.performed += OnSprint;
        controls.PlayerMap.Sprint.canceled += OnSprint;

        controls.PlayerMap.Roll.performed += OnRoll;
        controls.PlayerMap.Skill.performed += OnSkill;
    }

    private void OnDisable()
    {
        controls.PlayerMap.Move.performed -= OnMove;
        controls.PlayerMap.Move.canceled -= OnMove;

        controls.PlayerMap.Sprint.performed -= OnSprint;
        controls.PlayerMap.Sprint.canceled -= OnSprint;

        controls.PlayerMap.Roll.performed -= OnRoll;
        controls.PlayerMap.Skill.performed -= OnSkill;

        controls.Disable();
    }

    private void Update()
    {
        MovePlayer();
        FaceMouse();
    }

    private void MovePlayer()
    {
        if (playerRoll != null && playerRoll.IsRolling)
            return;

        Vector3 move = new Vector3(moveInput.x, 0f, moveInput.y);
        move = Vector3.ClampMagnitude(move, 1f);

        bool isMoving = move.sqrMagnitude > 0.01f;
        bool isSprinting = allowSprint && sprintHeld && isMoving;

        float speed = (isSprinting ? sprintSpeed : moveSpeed) * speedMultiplier;

        controller.Move(move * speed * Time.deltaTime);

        if (isSprinting != wasSprinting)
        {
            wasSprinting = isSprinting;
            OnSprintStateChanged?.Invoke(isSprinting);
        }
    }

    private void FaceMouse()
    {
        if (mainCamera == null || Mouse.current == null)
            return;

        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        Plane groundPlane = new Plane(Vector3.up, transform.position);

        if (groundPlane.Raycast(ray, out float distance))
        {
            Vector3 mouseWorldPos = ray.GetPoint(distance);
            Vector3 lookDir = mouseWorldPos - transform.position;
            lookDir.y = 0f;

            if (lookDir.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.LookRotation(lookDir);
        }
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    private void OnSprint(InputAction.CallbackContext context)
    {
        sprintHeld = context.ReadValueAsButton();
    }

    private void OnRoll(InputAction.CallbackContext context)
    {
        if (playerRoll != null)
            playerRoll.Roll(moveInput);
    }

    private void OnSkill(InputAction.CallbackContext context)
    {
        if (playerSkill != null)
            playerSkill.ActivateSkill();
    }

    public void SetSpeedMultiplier(float multiplier)
    {
        speedMultiplier = multiplier;
    }
}