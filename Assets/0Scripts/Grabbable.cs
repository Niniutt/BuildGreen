using UnityEngine;

public class Grabbable : MonoBehaviour
{
    public bool grabbable = true; // Means it's not picked up
    public Transform follow;
    public Type type;

    private void Update()
    {
        if (!grabbable && follow.transform) // picked up
        {
            // Follow the object to follow => Player's grabber
            transform.position = follow.transform.position;
            transform.rotation = follow.transform.rotation;
        }
    }

    public void UpdateGrabbable()
    {
        if (grabbable) {
            grabbable = false;
        }
        else grabbable = true;
    }
}
