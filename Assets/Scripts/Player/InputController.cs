using System;
using UnityEngine;
using UnityEngine.AI;

public class InputController : MonoBehaviour
{
    private GameInput gameInput;
    [SerializeField] private GameObject toggleableFlashlight;
    private Vector2 moveInput;
    public event Action<Vector2> OnMoveInput;
    public event Action OnInteractInput;
    public bool isWalking;
    public bool tryToRun;
    private void Awake()
    {
        gameInput = new GameInput();
    }
    private void OnEnable()
    {
        gameInput.Enable();
        gameInput.Player.ToggleFlashlight.performed += ToggleFlashlight;
        gameInput.Player.Move.performed += ReadMoveInput;
        gameInput.Player.Move.canceled += StopMovement;
        gameInput.Player.Sprint.performed += TryToRun;
        gameInput.Player.Sprint.canceled += StopRunning;
        gameInput.Player.Interact.performed += TryToInteract;
    }
    private void OnDisable()
    {
        gameInput.Player.ToggleFlashlight.performed -= ToggleFlashlight;
        gameInput.Player.Move.performed -= ReadMoveInput;
        gameInput.Player.Move.canceled -= StopMovement;
        gameInput.Player.Sprint.performed -= TryToRun;
        gameInput.Player.Sprint.canceled -= StopRunning;
        gameInput.Player.Interact.performed -= TryToInteract;
        gameInput.Disable();
    }
    private void ToggleFlashlight(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        toggleableFlashlight.SetActive(!toggleableFlashlight.activeSelf);
    }
    private void ReadMoveInput(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        isWalking = true;
        moveInput = context.ReadValue<Vector2>();
        OnMoveInput?.Invoke(moveInput);
    }
    private void StopMovement(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        isWalking = false;
        moveInput = Vector2.zero;
        OnMoveInput?.Invoke(moveInput);
    }
    private void TryToRun(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        tryToRun = true;
    }
    private void StopRunning(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        tryToRun = false;
    }
    private void TryToInteract(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        OnInteractInput?.Invoke();
    }
}
