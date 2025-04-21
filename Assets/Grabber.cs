using UnityEngine;

public class Grabber : MonoBehaviour
{
    public bool hasGrabbed = false;
    public bool inZone = false;
    public GameObject objectInZone;
    
    private void OnTriggerStay(Collider other)
    {
        // Update objectInZone only if there is no object already grabbed
        if (!hasGrabbed)
        {
            inZone = true;
            objectInZone = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!hasGrabbed)
        {
            // Reset data
            inZone = false;
            objectInZone = null;
        }
    }

    // Does it work correctly? How do we know if player has let go?
}
