using UnityEngine;

public class LadderClimb : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var moveScript = other.GetComponent<PlayerMovement>();
            if (moveScript != null) moveScript.isClimbing = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var moveScript = other.GetComponent<PlayerMovement>();
            if (moveScript != null) moveScript.isClimbing = false;
        }
    }
}