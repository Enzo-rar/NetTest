using UnityEngine;
using UnityEngine.InputSystem;

public class LocalPlayerInput : MonoBehaviour, IPlayerInputProvider
{
    private PlayerInput controls;
    private PlayerInputData currentInput;

    private void Awake()
    {
        controls = new PlayerInput();
    }

    private void OnEnable() => controls.Enable();
    private void OnDisable() => controls.Disable();

    public PlayerInputData GetInput()
    {
        
        currentInput.Move = controls.Player.Move.ReadValue<Vector2>();
        currentInput.Look = controls.Player.Look.ReadValue<Vector2>();

        
        currentInput.Jump = controls.Player.Jump.IsPressed();
        currentInput.Crouch = controls.Player.Crouch.IsPressed();
        currentInput.Sprint = controls.Player.Sprint.IsPressed();

        return currentInput;
    }
}