using UnityEngine;

// 
public class MiniGameZone : MonoBehaviour
{
    [SerializeField] private MiniGameType miniGameType = MiniGameType.NULL;
    [SerializeField] private Canvas miniGameCanvas;
    [SerializeField] private MiniGame miniGame;

    private Grabbable grab;

    public bool start = false;

    private void OnTriggerEnter(Collider other)
    {
        // if (!IsOwner) Debug.Log("MiniGameZone OnTriggerEnter: ");
        grab = other.GetComponent<Grabbable>();
        if (grab != null && grab.isGrabbed.Value)
        {
            // Check if the grabbable object is of the correct type for this mini-game
            if (grab.miniGameType.Value == miniGameType)
            {
                start = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            start = false;
            grab = null;
        }
    }

    private void Update()
    {
        if (start && Input.GetKeyDown(KeyCode.F))
        {
            // Signal player to stop input
            Debug.Log("Mini-game " + miniGameType + " started.");
            // Show the mini-game canvas
            miniGameCanvas.gameObject.SetActive(true);


            start = false; // Reset start to prevent multiple triggers
        }
    }
}
