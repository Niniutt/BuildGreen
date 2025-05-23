using Unity.Netcode;
using UnityEngine;

public class DestroyAfter : NetworkBehaviour
{
    private readonly float destroyTime = 3f;
    private float timer = 0f;
    private bool isDespawned = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        timer = destroyTime;
    }

    // Update is called once per frame
    void Update()
    {
        if (isDespawned) return;
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            isDespawned = true;
            if (IsServer)
                GetComponent<NetworkObject>().Despawn();
            else
                DestroyObjectServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void DestroyObjectServerRpc()
    {
        if (!IsServer) return;
        NetworkObject no = GetComponent<NetworkObject>();
        if (no) no.Despawn();
    }
}
