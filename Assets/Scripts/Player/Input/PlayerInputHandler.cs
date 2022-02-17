using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour, PlayerInputActions.IPlayerActions {
    public Vector2 MoveInput { get; private set; }

    public void OnGuard(InputAction.CallbackContext context) {
        Debug.Log("Implement Guard!");
    }

    public void OnJump(InputAction.CallbackContext context) {
        Debug.Log("Implement Jump!");
    }

    public void OnMovement(InputAction.CallbackContext context) {
        Debug.Log(context);
        MoveInput = context.ReadValue<Vector2>();    
    }
}
