using UnityEngine;

public class Grabbable : MonoBehaviour
{
    public bool grabbable = true;

    [SerializeField] GridManager gridManager;

    private Vector3 snappedPosition;

    private void Start()
    {
        gridManager = GameObject.FindWithTag("GridTag").GetComponent<GridManager>();
    }

    public void UpdateGrabbable()
    {
        if (grabbable) {
            grabbable = false;
        }
        else grabbable = true;
    }
}
