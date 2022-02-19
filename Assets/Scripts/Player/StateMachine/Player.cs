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

    public Rigidbody body;
    public Vector3 velocity, desiredVelocity;
    public float minGroundDotProduct;

    private void OnValidate() {
        minGroundDotProduct = Mathf.Cos(playerData.maxGroundAngle * Mathf.Deg2Rad);    
    }

    private void Awake() {
        StateMachine = new PlayerStateMachine();

        IdleState = new PlayerIdleState(this, playerData, StateMachine, "idle");
        MoveState = new PlayerMoveState(this, playerData, StateMachine, "move");
        
        body = GetComponent<Rigidbody>();
        OnValidate();
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
        // Velocity initalized for PhysicsUpdate
        velocity = body.velocity;
        
        StateMachine.CurrentState.PhysicsUpdate();

        // Apply velocity changes from PhysicsUpdate
        body.velocity = velocity;
    }

    private void OnCollisionEnter(Collision collision) {
        EvaluateCollision(collision);
    }

    private void OnCollisionExit(Collision collision) {
        EvaluateCollision(collision);   
    }

    public void EvaluateCollision(Collision collision) {
        for (int i = 0; i < collision.contactCount; i++) {
            Vector3 normal = collision.GetContact(i).normal;
            onGround |= normal.y > 0.9f;
        }
    }
}
