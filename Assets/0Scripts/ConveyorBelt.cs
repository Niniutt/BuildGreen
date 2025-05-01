using System.Collections.Generic;
using UnityEngine;

public class ConveyorBelt : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private Vector3 direction;
    [SerializeField] private List<GameObject> onBelt;
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < onBelt.Count; i++)
        {
            onBelt[i].transform.position += speed * direction * Time.deltaTime;
        }
    }

    // When something collides with the belt
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
