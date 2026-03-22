using UnityEngine;

public class GrassDetector : MonoBehaviour
{
    [Header("Encounter Settings")]
    public LayerMask grassLayer; 
    [Range(0f, 100f)] public float encounterChance = 1f; 
    public float stepInterval = 0.5f;   

    private CharacterController controller;
    private bool isInGrass = false;
    private float stepTimer;

    void Start() => controller = GetComponent<CharacterController>();

    void Update()
    {
        // --- THE FIX ---
        // Start the ray 2 units up (Head height) and shoot it 2.5 units down
        // This ensures the laser starts ABOVE the tall grass
        Vector3 rayOrigin = transform.position + Vector3.up * 2f;
        bool touchingGrass = Physics.Raycast(rayOrigin, Vector3.down, 2.5f, grassLayer);

        // Visual Debug: Red line in Scene view shows exactly what the laser sees
        Debug.DrawRay(rayOrigin, Vector3.down * 2.5f, touchingGrass ? Color.green : Color.red);

        if (touchingGrass && !isInGrass)
        {
            isInGrass = true;
            Debug.Log("<color=green>Entered Tall Grass</color>");
        }
        else if (!touchingGrass && isInGrass)
        {
            isInGrass = false;
            Debug.Log("<color=red>Left Tall Grass</color>");
        }

        // Encounter Logic
        if (isInGrass && controller.velocity.magnitude > 0.1f)
        {
            stepTimer += Time.deltaTime;
            if (stepTimer >= stepInterval)
            {
                stepTimer = 0f;
                if (Random.Range(0f, 100f) <= encounterChance)
                    Debug.Log("<color=yellow>A wild POKEMON appeared!</color>");
            }
        }
    }
}