using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
struct ObjectPosition
{
    public GameObject go;
    public Type type;
    public Vector2 v2;

    public ObjectPosition(GameObject go, Type type, Vector2 v2)
    {
        this.go = go;
        this.type = type;
        this.v2 = v2;
    }
}


class GridManager : MonoBehaviour
{
    [SerializeField] private Grid m_Grid;
    private float minX = -8f;
    private float maxX = 8f;
    private float minZ = -9f;
    private float maxZ = 9f;
    [SerializeField] private List<ObjectPosition> occupiedPositions = new();
    [SerializeField] private Vector3 gridCenterOffset = new Vector3(0.5f, 0, 0.5f);
    private float yOffset = 0.5f;

    private void Update()
    {
        // Update their positions
        for (int i = 0; i < occupiedPositions.Count; i++)
        {
            Vector3 position = occupiedPositions[i].go.transform.position;
            Vector3 snappedPosition = GetSnappedPosition(position);
            Vector2 v2 = To2(snappedPosition);

            occupiedPositions[i] = new ObjectPosition(occupiedPositions[i].go, occupiedPositions[i].type, v2);
        }
    }

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

    public void Add(GameObject go, Type type, Vector3 position)
    {
        occupiedPositions.Add(new ObjectPosition(go, type, To2(position)));
    }

    public void Remove(GameObject go, Type type, Vector3 position)
    {
        occupiedPositions.Remove(new ObjectPosition(go, type, To2(position)));
    }
}
