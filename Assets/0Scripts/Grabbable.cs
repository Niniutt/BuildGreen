using UnityEngine;

public class Grabbable : MonoBehaviour
{
    public bool grabbable = true;

    public void UpdateGrabbable()
    {
        if (grabbable) grabbable = false;
        else grabbable = true;
    }
}
