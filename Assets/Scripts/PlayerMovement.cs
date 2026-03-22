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
    public float rayDistance = 0.5f; // Keep this small (0.4 or 0.5)

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
        // We use the controller's built-in check for gravity so you don't "slow down" mid-air
        isGrounded = controller.isGrounded;

        if (isGrounded && playerVelocity.y < 0)
        {
            // Only resets gravity when the capsule physically touches the floor
            playerVelocity.y = -2f; 
        }

        // 2. SLOPE DETECTION (Raycast)
        // We still use the raycast to find the angle of the ground for smooth walking
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
            
            // If the ray sees a slope, tilt our movement to match it
            if (rayHitGround) 
            {
                move = Vector3.ProjectOnPlane(move, hit.normal).normalized;
            }
        }

        // 4. JUMP LOGIC
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            playerVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // 5. APPLY FINAL MOVEMENT
        playerVelocity.y += gravity * Time.deltaTime;
        
        // We combine horizontal speed and vertical gravity into one Move call
        Vector3 finalMove = (move * speed) + new Vector3(0, playerVelocity.y, 0);
        controller.Move(finalMove * Time.deltaTime);
    }
}