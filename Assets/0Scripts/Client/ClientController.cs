using UnityEngine;
using Unity.Netcode;

public class ClientController : NetworkBehaviour
{
    [SerializeField] private HostController m_HostController;

    private void Awake()
    {
        m_HostController.enabled = false;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsOwner)
        {
            m_HostController.enabled = true;
        }
    }
}
