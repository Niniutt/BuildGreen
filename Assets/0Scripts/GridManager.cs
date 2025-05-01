using System.Collections.Generic;
using UnityEngine;

struct ObjectPosition
{
    public GameObject go;
    public Vector2 v2;

    public ObjectPosition(GameObject go, Vector2 v2)
    {
        this.go = go;
        this.v2 = v2;
    }
}

public class GridManager : MonoBehaviour
{
    [SerializeField] private Grid m_Grid;
    private float minX = -8f;
    private float maxX = 8f;
    private float minZ = -9f;
    private float maxZ = 9f;
    private List<ObjectPosition> occupiedPositions = new();
    private Vector3 gridCenterOffset = new Vector3(0.5f, 0, 0.5f);
    private float yOffset = 0.5f;

    public Vector3 To3(Vector2 v2) => new Vector3(v2.x, yOffset, v2.y); // Kinda
    public Vector2 To2(Vector3 v3) => new Vector2(v3.x, v3.z);

    public Vector3 GetSnappedPosition(Vector3 point)
    {
        Vector3 snappedPosition = m_Grid.LocalToCell(point);
        snappedPosition += gridCenterOffset;
        snappedPosition.y = yOffset;
        return snappedPosition;
    }

    // Checks if the position of the (ungrabbed) item is in bounds and not already occupied
    public bool CheckPosition(Vector3 position)
    {
        // In bounds
        if(position.x > minX && position.x < maxX && position.z > minZ && position.z < maxZ)
        {
            // Not already occupied
            foreach (ObjectPosition spot in occupiedPositions)
            {
                if (spot.v2 == To2(position)) return true;
            }
        }
        return false;

    }

    public void AddPosition(GameObject go, Vector3 position)
    {
        occupiedPositions.Add(new ObjectPosition(go, To2(position)));
    }

    public void RemovePosition(GameObject go, Vector3 position)
    {
        occupiedPositions.Remove(new ObjectPosition(go, To2(position)));
    }
}
