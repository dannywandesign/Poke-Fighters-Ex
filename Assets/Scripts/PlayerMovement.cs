using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 30f;
    public float rotationSpeed = 720f;
    public float jumpForce = 12f; 
    public float gravityMultiplier = 5f;

    private Rigidbody rb;
    private Transform camTransform;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        camTransform = Camera.main.transform;
        
        // Keep the capsule upright
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    void Update()
    {
        // 1. Simple Grounded Check
        isGrounded = Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, 1.2f);

        // 2. Movement Logic
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 forward = camTransform.forward;
        Vector3 right = camTransform.right;
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        Vector3 moveDirection = (forward * v + right * h).normalized;

        if (moveDirection != Vector3.zero)
        {
            // Rotate to face movement
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            
            // Set velocity directly for Speed 30 snappy movement
            Vector3 vel = moveDirection * speed;
            vel.y = rb.linearVelocity.y;
            rb.linearVelocity = vel;
        }
        else if (isGrounded)
        {
            // Instant stop when keys are released
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        }

        // 3. Jump Logic
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    void FixedUpdate()
    {
        // 4. Strong Gravity
        // This keeps you on the ground at high speeds
        if (!isGrounded)
        {
            rb.AddForce(Vector3.down * gravityMultiplier * 9.81f, ForceMode.Acceleration);
        }
    }
}