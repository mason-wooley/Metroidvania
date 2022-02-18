using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour {
    public PlayerStateMachine StateMachine { get; private set; }

    public PlayerIdleState IdleState { get; private set; }
    public PlayerMoveState MoveState { get; private set; }
    
    public Animator Anim { get; private set; }
    
    public PlayerInputHandler Input { get; private set; }

    [SerializeField]
    private PlayerData playerData;

    private void Awake() {
        StateMachine = new PlayerStateMachine();

        IdleState = new PlayerIdleState(this, playerData, StateMachine, "idle");
        MoveState = new PlayerMoveState(this, playerData, StateMachine, "move");
    }

    private void Start() {
        Anim = GetComponent<Animator>();
        Input = GetComponent<PlayerInputHandler>();

        StateMachine.Initialize(IdleState);
    }

    private void Update() {
        StateMachine.CurrentState.LogicUpdate();
    }

    private void FixedUpdate() {
        StateMachine.CurrentState.PhysicsUpdate();
    }

}
