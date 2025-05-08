using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct ObjectPosition
{
    public GameObject go;
    public Grabbable grabbable;
    public Type type;
    public Vector2 v2;

    public ObjectPosition(GameObject go, Type type, Vector2 v2)
    {
        this.go = go;
        this.grabbable = go.GetComponent<Grabbable>();
        this.type = type;
        this.v2 = v2;
    }
}

public class GridManager : MonoBehaviour
{
    [SerializeField] private Grid m_Grid;
    [SerializeField] private LevelManager levelManager;
    private float minX = -8f;
    private float maxX = 8f;
    private float minZ = -9f;
    private float maxZ = 9f;
    private List<ObjectPosition> occupiedPositions = new();
    private List<int> assemblyCandidates = new();
    private Vector3 gridCenterOffset = new Vector3(0.5f, 0, 0.5f);
    private float yOffset = 0.5f;

    #region PRIVATE METHODS

    private void Update()
    {
        // Update their positions
        for (int i = 0; i < occupiedPositions.Count; i++)
        {
            if (occupiedPositions[i].grabbable.grabbable)
            {
                // Update position
                Vector3 position = occupiedPositions[i].go.transform.position;
                Vector3 snappedPosition = GetSnappedPosition(position);
                Vector2 v2 = To2(snappedPosition);

                occupiedPositions[i] = new ObjectPosition(occupiedPositions[i].go, occupiedPositions[i].type, v2);
            }
        }
    }

    #endregion
    #region PUBLIC METHODS

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
    // Returns true if position is occupied
    public bool CheckPosition(GameObject carried, Vector3 position)
    {
        // In bounds
        if(position.x > minX && position.x < maxX && position.z > minZ && position.z < maxZ)
        {
            // Not already occupied
            foreach (ObjectPosition spot in occupiedPositions)
            {
                // if (spot.go == carried) break; // Skip if spot is the carried object
                if (spot.v2 == To2(position)) return true;
            }
        }
        return false;
    }

    public void Add(GameObject go, Type type, Vector3 position)
    {
        occupiedPositions.Add(new ObjectPosition(go, type, To2(position)));
    }

    public void Remove(GameObject go, Type type)
    {
        occupiedPositions.RemoveAll(op => op.go == go && op.type == type);
    }

    public void Remove(ObjectPosition op)
    {
        occupiedPositions.Remove(op);
    }

    public List<Type> GetAssemblyCandidates()
    {
        assemblyCandidates.Clear();

        List<Type> output = new List<Type>();
        for (int i = 0; i < occupiedPositions.Count; i++)
        {
            Vector2 v2 = occupiedPositions[i].v2;
            // Take only objects on the assembly bench
            if (v2.x < 1 && v2.x > -1 && v2.y < 1 && v2.y > -1)
            {
                output.Add(occupiedPositions[i].type);
                assemblyCandidates.Add(i);
            }
        }
        return output;
    }

    public void Assemble(Type output)
    {
        // Destroy items
        for (int i = 0; i < assemblyCandidates.Count; i++)
        {
            int index = assemblyCandidates[i];
            ObjectPosition op = occupiedPositions[index];
            DestroyImmediate(op.go);
        }
        // We have to remove all the ops in the end otherwise they disturb the indexes (from end to start)
        for (int i = assemblyCandidates.Count - 1; i >= 0; i--)
        {
            int index = assemblyCandidates[i];
            ObjectPosition op = occupiedPositions[index];
            Remove(op);
        }

        // Create output item
        levelManager.SpawnCraftedItem(output);
    }

    #endregion
}
