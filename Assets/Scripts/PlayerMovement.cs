using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 20f;
    public float rotationSpeed = 720f;
    public float jumpHeight = 3f;
    public float gravity = -40f;

    [Header("Detection Settings")]
    public LayerMask groundLayer;
    public float rayDistance = 0.5f;
    public float groundCheckDistance = 0.2f; 

    [Header("Ladder State")]
    public bool isClimbing = false;

    private CharacterController controller;
    private Transform camTransform;
    private Vector3 playerVelocity;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        camTransform = Camera.main.transform;
    }

    void Update()
    {
        // 1. ADVANCED GROUND & SLOPE CHECK
        RaycastHit groundHit;
        // We use a SphereCast downward to find the angle of the surface we are touching
        bool hitSomething = Physics.SphereCast(transform.position, controller.radius, Vector3.down, out groundHit, groundCheckDistance, groundLayer);
        
        bool isOnValidSlope = false;
        if (hitSomething)
        {
            // Calculate the angle between the ground normal and the Up direction
            float slopeAngle = Vector3.Angle(Vector3.up, groundHit.normal);
            // Only count as "Ground" if the angle is walkable (less than slopeLimit)
            if (slopeAngle <= controller.slopeLimit)
            {
                isOnValidSlope = true;
            }
        }

        // The player is "Actually Grounded" only if they are on a valid, non-vertical slope
        bool actuallyGrounded = (controller.isGrounded || isOnValidSlope);

        if (actuallyGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = -5f; 
        }

        // 2. INPUT & ROTATION
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 forward = camTransform.forward;
        Vector3 right = camTransform.right;
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        Vector3 move = (forward * v + right * h).normalized;

        if (move != Vector3.zero)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(move), rotationSpeed * Time.deltaTime);
            
            // Re-use the groundHit normal to project movement onto the slope
            if (hitSomething && isOnValidSlope)
            {
                move = Vector3.ProjectOnPlane(move, groundHit.normal).normalized;
            }
        }

        // 3. CONTINUOUS JUMP LOGIC
        // This will now fail if you are touching a vertical wall because isOnValidSlope will be false
        if (Input.GetKey(KeyCode.Space) && actuallyGrounded && !isClimbing)
        {
            playerVelocity.y = 0f; 
            playerVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // 4. LADDER & GRAVITY LOGIC
        if (isClimbing)
        {
            float vInput = Input.GetAxisRaw("Vertical");
            if (vInput > 0.1f)
            {
                playerVelocity.y = vInput * speed * 0.5f;
            }
            else
            {
                playerVelocity.y += gravity * Time.deltaTime;
                playerVelocity.y = Mathf.Max(playerVelocity.y, -5f);
            }
        }
        else
        {
            playerVelocity.y += gravity * Time.deltaTime;
        }
        
        // 5. FINAL MOVEMENT
        Vector3 finalMove = (move * speed) + new Vector3(0, playerVelocity.y, 0);
        controller.Move(finalMove * Time.deltaTime);
    }
}