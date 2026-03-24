using UnityEngine;

public class LadderClimb : MonoBehaviour
{
    public float climbSpeed = 5f;

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // 1. Get the movement script to toggle gravity
            PlayerMovement moveScript = other.GetComponent<PlayerMovement>();
            CharacterController controller = other.GetComponent<CharacterController>();

            if (moveScript != null && controller != null)
            {
                moveScript.isClimbing = true; // Stop the gravity!

                float input = Input.GetAxis("Vertical");
                Vector3 move = Vector3.up * input * climbSpeed;
                controller.Move(move * Time.deltaTime);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerMovement moveScript = other.GetComponent<PlayerMovement>();
            if (moveScript != null)
            {
                moveScript.isClimbing = false; // Turn gravity back on
            }
        }
    }
}