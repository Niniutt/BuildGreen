using UnityEngine;

public class Grabbable : MonoBehaviour
{
    public bool grabbable = true; // Means it's not picked up
    public Type type;

    public void UpdateGrabbable()
    {
        if (grabbable) {
            grabbable = false;
        }
        else grabbable = true;
    }
}
