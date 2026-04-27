using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerInputInvoker : MonoBehaviour
{
    [Header("Invoke Events")]
    public UnityEvent OnCast;
    public UnityEvent OnPrimaryWeapon;
    public UnityEvent OnSecondaryWeapon;

    private MovementControls controls;

    private void Awake()
    {
        controls = new MovementControls();
    }

    private void OnEnable()
    {
        controls.Enable();

        controls.PlayerMap.Cast.performed += Cast;
        controls.PlayerMap.PrimaryWeapon.performed += PrimaryWeapon;
        controls.PlayerMap.SecondaryWeapon.performed += SecondaryWeapon;
    }

    private void OnDisable()
    {
        controls.PlayerMap.Cast.performed -= Cast;
        controls.PlayerMap.PrimaryWeapon.performed -= PrimaryWeapon;
        controls.PlayerMap.SecondaryWeapon.performed -= SecondaryWeapon;

        controls.Disable();
    }

    private void Cast(InputAction.CallbackContext context)
    {
        OnCast?.Invoke();
    }

    private void PrimaryWeapon(InputAction.CallbackContext context)
    {
        OnPrimaryWeapon?.Invoke();
    }

    private void SecondaryWeapon(InputAction.CallbackContext context)
    {
        OnSecondaryWeapon?.Invoke();
    }
}