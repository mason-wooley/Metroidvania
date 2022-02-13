using UnityEngine;

public class PlayerMovement : MonoBehaviour {
    // Serialized fields
    [Range(0f, 100f)] public float maxVelocity = 10f;
    [Range(0f, 100f)] public float maxAcceleration = 10f, maxAirAcceleration = 1f;
    [Range(0f, 10f)] public float jumpHeight = 2f;
    [Range(0, 10)] public int maxAirJumps = 0;
    [Range(0f, 90f)] public float maxGroundAngle = 25f, maxStairsAngle = 50f;
    [Range(0f, 100f)] public float maxSnapSpeed = 100f;
    [Min(0f)] public float probeDistance = 2f;
    public LayerMask probeMask = -1, stairsMask = -1;

    [SerializeField]
    Transform playerInputSpace = default;

    private bool jumpRequest;
    private int groundContactCount, steepContactCount;
    private int jumpPhase;
    private int stepsSinceLastGrounded, stepsSinceLastJump;

    private bool OnGround => groundContactCount > 0;

    private bool OnSteep => steepContactCount > 0;

    // Physics vars
    private Rigidbody body;
    private Vector3 velocity, desiredVelocity;
    private float minGroundDotProduct, minStairsDotProduct;
    private Vector3 contactNormal, steepNormal;

    Vector3 upAxis, rightAxis;

    void OnValidate() {
        minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
        minStairsDotProduct = Mathf.Cos(maxStairsAngle * Mathf.Deg2Rad);
    }

    private void Awake() {
        body = GetComponent<Rigidbody>();
        OnValidate();
    }

    void Update() {
        // Get inputs
        float xInput = Input.GetAxis("Horizontal");

        if (playerInputSpace) {
            rightAxis = ProjectDirectionOnPlane(playerInputSpace.right, upAxis);

        } else {
            rightAxis = ProjectDirectionOnPlane(Vector3.right, upAxis);
        }
        
        // Used in next FixedUpdate call to control velocity
        desiredVelocity = new Vector3(xInput, 0f, 0f) * maxVelocity;
        
        jumpRequest |= Input.GetButtonDown("Jump");
    }

    void FixedUpdate() {
        upAxis = -Physics.gravity.normalized;

        UpdateState();
        AdjustVelocity();
        
        // Jump command
        if (jumpRequest) {
            jumpRequest = false;
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
