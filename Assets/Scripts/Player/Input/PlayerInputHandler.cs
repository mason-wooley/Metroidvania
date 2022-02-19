using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour, PlayerInputActions.IPlayerActions {
    public Vector2 Movement { get; private set; }
    public float Guard { get; private set; }
    public float Jump { get; private set; }

    private PlayerInputActions inputActions;

    private void Awake() {
        inputActions = new PlayerInputActions();
        inputActions.Player.SetCallbacks(this);    
    }

    private void OnEnable() {
        inputActions.Enable();
    }

    private void OnDisable() {
        inputActions.Disable();
    }

    public void OnGuard(InputAction.CallbackContext context) {
        Guard = context.ReadValue<float>();    
    }

    public void OnJump(InputAction.CallbackContext context) {
        Jump = context.ReadValue<float>();
    }

    public void OnMovement(InputAction.CallbackContext context) {
        Movement = context.ReadValue<Vector2>();    
    }
}
