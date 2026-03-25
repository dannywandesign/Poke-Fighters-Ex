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

    [Header("Ladder State")]
    public bool isClimbing = false; // The LadderClimb script will turn this on/off

    private CharacterController controller;
    private Transform camTransform;
    private Vector3 playerVelocity;
    private bool isGrounded;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        camTransform = Camera.main.transform;
    }

    void Update()
    {
        // 1. PHYSICAL GROUND CHECK
        isGrounded = controller.isGrounded;

        if (isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = -2f; 
        }

        // 2. SLOPE DETECTION
        RaycastHit hit;
        bool rayHitGround = Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, out hit, rayDistance, groundLayer, QueryTriggerInteraction.Ignore);

        // 3. INPUT & ROTATION
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
            
            if (rayHitGround) 
            {
                move = Vector3.ProjectOnPlane(move, hit.normal).normalized;
            }
        }

        // 4. JUMP LOGIC (Disabled while climbing)
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && !isClimbing)
        {
            playerVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // 5. LADDER & GRAVITY LOGIC
        if (isClimbing)
        {
            float vInput = Input.GetAxisRaw("Vertical");

            if (vInput > 0.1f) 
            {
                // We are pressing "Up" - Move up and ignore gravity
                playerVelocity.y = vInput * speed * 0.5f; 
            }
            else 
            {
                // We are NOT pressing "Up" - Let gravity pull us down the ladder
                playerVelocity.y += gravity * Time.deltaTime;
                
                // Optional: Clamp sliding speed so you don't fall too fast 
                playerVelocity.y = Mathf.Max(playerVelocity.y, -5f); 
            }
        }
        else
        {
            // Normal gravity when not on a ladder
            playerVelocity.y += gravity * Time.deltaTime;
        }
        
        // 6. FINAL MOVEMENT
        Vector3 finalMove = (move * speed) + new Vector3(0, playerVelocity.y, 0);
        controller.Move(finalMove * Time.deltaTime);
    }
}