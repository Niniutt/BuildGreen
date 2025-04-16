using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [SerializeField] private Grid m_Grid;

    [SerializeField] private GameObject m_Object;
    [SerializeField] private GameObject m_GhostObject;

    private HashSet<Vector3> occupiedPositions = new HashSet<Vector3>();

    private void Start()
    {
        
    }

    private void Update()
    {
        
    }

    /* private void Start()
    {
        CreateGhostObject();
    }

    private void Update()
    {
        UpdateGhostPosition();

        if (Input.GetMouseButtonDown(0))
        {
            PlaceObject();
        }
    }

    void CreateGhostObject ()
    {
        m_GhostObject = Instantiate(m_Object);
        m_GhostObject.GetComponent<Collider>().enabled = false;

        Renderer[] renderers = m_GhostObject.GetComponentsInChildren<Renderer>();

        foreach (Renderer renderer in renderers)
        {
            Material mat = renderer.material;
            Color color = mat.color;
            color.a = 0.5f;
            mat.color = color;
            renderer.material = mat;

            // mat things
            mat.SetFloat("_Mode", 2);
            mat.SetInt("_ScrBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY");
            mat.renderQueue = 3000;

        }
    }

    void UpdateGhostPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector3 point = hit.point;
            Vector3 snappedPosition = m_Grid.LocalToCell(point);
            
            m_GhostObject.transform.position = snappedPosition;

            if (occupiedPositions.Contains(snappedPosition))
                SetGhostColor(Color.red);
            else
                SetGhostColor(new Color(1f, 1f, 1f, 0.5f));
        }
    }

    void SetGhostColor (Color color)
    {
        Renderer[] renderers = m_GhostObject.GetComponentsInChildren<Renderer>();

        foreach (Renderer renderer in renderers)
        {
            Material mat = renderer.material;
            mat.color = color;
            renderer.material = mat;
        }
    }

    void PlaceObject()
    {
        Vector3 placementPosition = m_GhostObject.transform.position;
        
        if (!occupiedPositions.Contains(placementPosition))
        {
            Instantiate(m_Object, placementPosition, Quaternion.identity * Quaternion.Euler(Vector3.right * -90));

            occupiedPositions.Add(placementPosition);
        }
    }*/
}
