using System.Collections.Generic;
using UnityEngine;

public class ConveyorBelt : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private Vector3 direction;
    [SerializeField] private List<GameObject> onBelt;

    // Move forward objects on belt
    void Update()
    {
        List<int> indicesToRemove = new List<int>();
        for (int i = 0; i < onBelt.Count; i++)
        {
            if (onBelt[i] == null)
            {
                indicesToRemove.Add(i);
                break;
            }
            onBelt[i].transform.position += speed * direction * Time.deltaTime;
        }
        if (indicesToRemove.Count > 0)
        {
            for (int i = indicesToRemove.Count - 1; i >= 0; i--)
            {
                onBelt.RemoveAt(i);
            }
        }
    }

    // When something collides with the belt
    // Eventually this script should have the object move along a strict line / square instead of applying a velocity when in collider
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 6)
        {
            Grabbable grab = other.gameObject.GetComponent<Grabbable>();
            if (grab == null) Debug.LogError("Not valid grabbable object (missing 'Grabbable' script)");
            if (grab.grabbable)
            {
                onBelt.Add(other.gameObject);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        onBelt.Remove(other.gameObject);
    }
}
