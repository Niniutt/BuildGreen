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

    private void Update()
    {
        // Keep grid position in check in order to know the occupied positions
        if (!grabbable)
        {
            snappedPosition = gridManager.GetSnappedPosition(transform.position);
            gridManager.AddPosition(gameObject, snappedPosition);
        }
    }

    public void UpdateGrabbable()
    {
        if (grabbable) {
            grabbable = false;
            gridManager.RemovePosition(gameObject, snappedPosition);
        }
        else grabbable = true;
    }
}
