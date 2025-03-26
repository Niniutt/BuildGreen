using UnityEngine;
using Unity.Netcode;

public class HostController : NetworkBehaviour
{
    private CharacterController controller;

    public float speed = 20.0f;

    //void Start()
    public override void OnNetworkSpawn()
    {
        controller = GetComponent<CharacterController>();

        if (!IsOwner)
        {
            // Deactivate camera
        }
    }

    void Update()
    {
        if (!IsOwner)
        {
            return;
        }

        Vector3 move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        controller.Move(speed * Time.deltaTime * move);
    }
}
