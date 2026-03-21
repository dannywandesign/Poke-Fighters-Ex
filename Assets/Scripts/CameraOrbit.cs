using UnityEngine;

public class CameraOrbit : MonoBehaviour
{
    public Transform target;

    [Header("Sphere Settings")]
    public float distance = 5.0f;     // The radius of your sphere
    public float pivotHeight = 1.6f;  // Set this to 1.6 to look Charmander in the face
    
    [Header("Angle Settings")]
    [Range(-20, 85)]
    public float verticalAngle = 20f; // This is your "Height" on the sphere
    public float sensitivity = 3.0f;
    public LayerMask collisionLayers;

    private float currentX = 0.0f;
    private float currentY = 0.0f;
    private float currentDistance;

    void Start()
    {
        currentDistance = distance;
        
        // Grab starting rotation from the editor
        Vector3 angles = transform.eulerAngles;
        currentX = angles.y;
        currentY = verticalAngle; // Use the slider value for the start
    }

    void LateUpdate()
    {
        if (target == null) return;

        // 1. Right-click to rotate
        if (Input.GetMouseButton(1))
        {
            currentX += Input.GetAxis("Mouse X") * sensitivity;
            currentY -= Input.GetAxis("Mouse Y") * sensitivity;
            currentY = Mathf.Clamp(currentY, -10, 85); 
        }

        // 2. Define the CENTER of the sphere (The Face)
        Vector3 pivotPoint = target.position + Vector3.up * pivotHeight;

        // 3. Calculate the Spherical Position
        // currentY is the "Latitude", currentX is the "Longitude"
        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
        Vector3 selectionOffset = rotation * new Vector3(0, 0, -distance);
        Vector3 desiredPosition = pivotPoint + selectionOffset;

        // 4. Collision Check (Raycast from face to camera)
        RaycastHit hit;
        if (Physics.Linecast(pivotPoint, desiredPosition, out hit, collisionLayers))
        {
            currentDistance = Mathf.Clamp(hit.distance - 0.2f, 1.0f, distance);
        }
        else
        {
            currentDistance = distance;
        }

        // 5. Final Transform
        transform.position = pivotPoint + rotation * new Vector3(0, 0, -currentDistance);
        transform.LookAt(pivotPoint);
    }
}