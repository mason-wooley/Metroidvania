using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

enum PlayerStateEnum {
    IDLE,
    RUN,
    JUMP,
    AIR,
    GUARD,
    SLIDE,
    ROLL
}

public class PlayerMovement : MonoBehaviour {
    // Input object
    [SerializeField]
    PlayerInputActions playerControls;

    [SerializeField]
    Text playerStateText;

    [SerializeField]
    Text groundStateText;

    // Move speed settings
    [SerializeField, Range(0f, 100f)]
    float maxVelocity = 10f;
    
    [SerializeField, Range(0f, 100f)]
    float maxAcceleration = 10f, maxAirAcceleration = 1f;
    
    // Jump settings
    [SerializeField, Range(0f, 10f)]
    float jumpHeight = 2f;
    
    [SerializeField, Range(0, 10)]
    int maxAirJumps = 0;

    [SerializeField, Range(0f, 10f)]
    float guardDuration = 5f;

    [SerializeField, Range(0f, 10f)]
    float slideDuration = 2f;

    [SerializeField, Range(0f, 10f)]
    float rollDuration = 2f;

    // Sloped platform settings
    [SerializeField, Range(0f, 90f)]
    float maxGroundAngle = 25f, maxStairsAngle = 50f;
    
    [SerializeField, Range(0f, 100f)]
    float maxSnapSpeed = 100f;
    
    [SerializeField, Min(0f)]
    float probeDistance = 2f;
    
    [SerializeField]
    LayerMask probeMask = -1, stairsMask = -1;

    // Alternate movement axis settings
    [SerializeField]
    Transform playerInputSpace = default;

    // TODO: Make some sort of separate class for this, could get 
    private InputAction move;
    private InputAction jump;
    private InputAction guard;

    private PlayerStateEnum state;


    private bool jumpRequest;
    private bool crouching;
    private int groundContactCount, steepContactCount;
    private int jumpPhase;
    private int stepsSinceLastGrounded, stepsSinceLastJump;

    private bool OnGround => groundContactCount > 0;

    private bool OnSteep => steepContactCount > 0;

    private bool GuardPressed => guard.ReadValue<float>() > 0f;

    // Physics vars
    private Rigidbody body;
    private Vector3 velocity, desiredVelocity;
    private float minGroundDotProduct, minStairsDotProduct;
    private Vector3 contactNormal, steepNormal;

    Vector3 upAxis, rightAxis;

    void OnEnable () {
        move = playerControls.Player.Movement;
        move.Enable();

        jump = playerControls.Player.Jump;
        jump.Enable();

        guard = playerControls.Player.Guard;
        guard.Enable();
    }

    void OnDisable () {
        move.Disable();
        jump.Disable();
    }

    void OnValidate() {
        minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
        minStairsDotProduct = Mathf.Cos(maxStairsAngle * Mathf.Deg2Rad);
    }

    private void Awake() {
        playerControls = new PlayerInputActions();
        body = GetComponent<Rigidbody>();
        OnValidate();
    }

    void Update() {
        // Get inputs
        Vector2 moveDirection = move.ReadValue<Vector2>();

        if (playerInputSpace) {
            rightAxis = ProjectDirectionOnPlane(playerInputSpace.right, upAxis);
        } else {
            rightAxis = ProjectDirectionOnPlane(Vector3.right, upAxis);
        }

        // Used in next FixedUpdate call to control velocity
        desiredVelocity = new Vector3(moveDirection.x, 0f, 0f) * maxVelocity;

        if (desiredVelocity.magnitude > 0.001f) {
            if (OnGround && state == PlayerStateEnum.IDLE) {
                state = PlayerStateEnum.RUN;
            } else if (OnGround && state == PlayerStateEnum.GUARD) {
                state = PlayerStateEnum.ROLL;
                Roll();
            } else if (OnGround && state == PlayerStateEnum.JUMP) {
                state = PlayerStateEnum.RUN;
            }
        } else if (desiredVelocity.magnitude < 0.001f && OnGround) {
            state = PlayerStateEnum.IDLE;
        }

        if (GuardPressed) {
            if (state == PlayerStateEnum.RUN) {
                state = PlayerStateEnum.SLIDE;
                Slide();
            } else if (state == PlayerStateEnum.IDLE) {
                state = PlayerStateEnum.GUARD;
                Guard();
            }
        }

        jumpRequest |= jump.WasPressedThisFrame();

        if (jumpRequest || !OnGround) {
            state = PlayerStateEnum.JUMP;
        }

        playerStateText.text = "PlayerState: " + state.ToString();

        if (OnGround) {
            groundStateText.text = "Grounded: True";
        } else {
            groundStateText.text = "Grounded: False";
        }
    }

    void FixedUpdate() {
        upAxis = -Physics.gravity.normalized;

        UpdateState();
        AdjustVelocity();
        
        // Jump command
        if (jumpRequest) {
            jumpRequest = false;
            state = PlayerStateEnum.JUMP;
            Jump();
        }

        body.velocity = velocity;
        ClearState();
    }

    void OnCollisionEnter(Collision collision) {
        EvaluateCollision(collision);
    }

    void OnCollisionStay(Collision collision) {
        EvaluateCollision(collision);
    }

    void Jump() {
        Vector3 jumpDirection;

        if (OnGround) {
            jumpDirection = contactNormal;
        } else if (OnSteep) {
            jumpDirection = steepNormal;
            jumpPhase = 0;
        } else if (maxAirJumps > 0 && jumpPhase <= maxAirJumps) {
            if (jumpPhase == 0) {
                jumpPhase = 1;
            }
            jumpDirection = contactNormal;
        } else {
            return;
        }
     
        stepsSinceLastJump = 0;
        jumpPhase += 1;

        float jumpSpeed = Mathf.Sqrt(2f * Physics.gravity.magnitude * jumpHeight);
        jumpDirection = (jumpDirection + upAxis).normalized;
        float alignedSpeed = Vector3.Dot(velocity, jumpDirection);

        if (alignedSpeed > 0f) {
            jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0f);
        }

        velocity += jumpDirection * jumpSpeed;
    }

    void Slide () {
        // TODO: Modify hitbox
        transform.rotation = Quaternion.Euler(new Vector3(270, 90));
        Invoke("EndSlide", slideDuration);
    }

    void EndSlide () {
        transform.rotation = Quaternion.Euler(new Vector3(0, 90));
    }

    void Roll () {
        // TODO: Modify hitbox
        transform.rotation = Quaternion.Euler(new Vector3(0, 90, 270));
        Invoke("EndRoll", rollDuration);
    }

    void EndRoll () {
        transform.rotation = Quaternion.Euler(new Vector3(0, 90, 0));
    }

    void Guard () {
        Debug.Log("I should be guarding!");
    }

    bool SnapToGround () {
        if (stepsSinceLastGrounded > 1 || stepsSinceLastJump <= 2) {
            return false;
        }

        float speed = velocity.magnitude;
        if (speed > maxSnapSpeed) {
            return false;
        }

        if (!Physics.Raycast(body.position, -upAxis, out RaycastHit hit, probeDistance, probeMask)) {
            return false;
        }

        float upDot = Vector3.Dot(upAxis, hit.normal);
        if (upDot < GetMinDot(hit.collider.gameObject.layer)) {
            return false;
        }

        groundContactCount = 1;
        contactNormal = hit.normal;
        float dot = Vector3.Dot(velocity, hit.normal);
        velocity = (velocity - hit.normal * dot).normalized * speed;
        return true;
    }

    void EvaluateCollision(Collision collision) {
        float minDot = GetMinDot(collision.gameObject.layer);
        for (int i = 0; i < collision.contactCount; i++) {
            Vector3 normal = collision.GetContact(i).normal;
            float upDot = Vector3.Dot(upAxis, normal);
            if (upDot >= minDot) {
                groundContactCount += 1;
                contactNormal += normal;
            } else if (upDot > -0.01f) {
                steepContactCount += 1;
                steepNormal += normal;
            }
        }
    }

    void UpdateState() {
        stepsSinceLastGrounded += 1;
        stepsSinceLastJump += 1;
        velocity = body.velocity;
        if (OnGround || SnapToGround() || CheckSteepContacts()) {
            stepsSinceLastGrounded = 0;
            if (stepsSinceLastJump > 1) {
                jumpPhase = 0;
            }
            if (groundContactCount > 1) { 
                contactNormal.Normalize();
            }
        } else {
            contactNormal = upAxis;
        }

        // Rotate player to match upAxis
        // TODO: Movement code breaks when gravity direction changes
        // transform.rotation = Quaternion.Euler(new Vector3(Mathf.Asin(upAxis.x) * Mathf.Rad2Deg, Mathf.Acos(upAxis.y) * Mathf.Rad2Deg, 0f));
    }

    void ClearState() {
        groundContactCount = steepContactCount = 0;
        contactNormal = steepNormal = Vector3.zero;
    }

    bool CheckSteepContacts () {
        if (steepContactCount > 1) {
            steepNormal.Normalize();
            float upDot = Vector3.Dot(upAxis, steepNormal);
            if (upDot >= minGroundDotProduct) {
                groundContactCount = 1;
                contactNormal = steepNormal;
                return true;
            }
        }
        return false;
    }

    float GetMinDot (int layer) {
        return (stairsMask & (1 << layer)) == 0 ? minGroundDotProduct : minStairsDotProduct;
    }

    Vector3 ProjectDirectionOnPlane (Vector3 direction, Vector3 normal) {
        return (direction - normal * Vector3.Dot(direction, normal)).normalized;
    }

    void AdjustVelocity () {
        Vector3 xAxis = ProjectDirectionOnPlane(rightAxis, contactNormal);

        float currentX = Vector3.Dot(velocity, xAxis);

        float acceleration = OnGround ? maxAcceleration : maxAirAcceleration;
        float maxSpeedChange = acceleration * Time.deltaTime;

        float newX = Mathf.MoveTowards(currentX, desiredVelocity.x, maxSpeedChange);

        velocity += xAxis * (newX - currentX);
    }
}
