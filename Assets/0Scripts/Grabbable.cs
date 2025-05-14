using Unity.Netcode;
using UnityEngine;

public class Grabbable : NetworkBehaviour
{
    public Transform follow;
    public Type type;

    public NetworkVariable<bool> isGrabbed = new(false);

    private void Update()
    {
        if (isGrabbed.Value && follow != null) // picked up
        {
            // Follow the object to follow => Player's grabber
            transform.position = follow.position;
            transform.rotation = follow.rotation;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void GrabServerRpc(ulong playerNetworkObjectId)
    {
        isGrabbed.Value = true;
        SetFollowClientRpc(playerNetworkObjectId);
    }

    [ServerRpc(RequireOwnership = false)]
    public void UngrabServerRpc()
    {
        isGrabbed.Value = false;
        ClearFollowClientRpc();
    }

    [ClientRpc]
    private void SetFollowClientRpc(ulong playerNetworkObjectId)
    {
        Debug.Log("ClientRpc SetFollowClientRpc");
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(playerNetworkObjectId, out var playerObj))
        {
            var hostController = playerObj.GetComponent<HostController>();
            if (hostController != null)
            {
                follow = hostController.grabberTransform;
            }
        }
    }

    [ClientRpc]
    private void ClearFollowClientRpc()
    {
        follow = null;
    }

    public void UpdateGrabbable()
    {
        isGrabbed.Value = !isGrabbed.Value;
    }
}
