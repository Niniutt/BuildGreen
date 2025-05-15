using Unity.Netcode;
using UnityEngine;

public class Grabbable : NetworkBehaviour
{
    public NetworkVariable<Type> type;
    public NetworkVariable<bool> isGrabbed = new(false);

    public NetworkVariable<Vector3> grabberPosition = new(Vector3.zero, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<Quaternion> grabberRotation = new(Quaternion.identity, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    private void Update()
    {
        if (isGrabbed.Value)
        {
            transform.position = grabberPosition.Value;
            transform.rotation = grabberRotation.Value;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void GrabServerRpc(ulong playerNetworkObjectId)
    {
        isGrabbed.Value = true;
    }

    [ServerRpc(RequireOwnership = false)]
    public void UngrabServerRpc()
    {
        isGrabbed.Value = false;
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestChangeOwnershipServerRpc(ulong newOwnerClientId)
    {
        GetComponent<NetworkObject>().ChangeOwnership(newOwnerClientId);
    }

    public void UpdateGrabberPose(Vector3 position, Quaternion rotation)
    {
        if (!IsOwner) return;
        grabberPosition.Value = position;
        grabberRotation.Value = rotation;
    }

    public void UpdateGrabbable()
    {
        isGrabbed.Value = !isGrabbed.Value;
    }
}
