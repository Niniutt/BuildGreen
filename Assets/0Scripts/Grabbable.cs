using Unity.Netcode;
using UnityEngine;

public class Grabbable : NetworkBehaviour
{
    public NetworkVariable<Type> type;
    public NetworkVariable<bool> isGrabbed = new(false);
    public NetworkVariable<bool> isChecked = new(false);
    public NetworkVariable<bool> isDeliveryReady = new(false);

    private NetworkVariable<Vector3> grabberPosition = new(Vector3.zero, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<Quaternion> grabberRotation = new(Quaternion.identity, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public NetworkVariable<Vector3> snappedPosition = new(Vector3.zero);

    private void Update()
    {
        if (isGrabbed.Value)
        {
            transform.position = grabberPosition.Value;
            transform.rotation = grabberRotation.Value;
        }
        else if (snappedPosition.Value != Vector3.zero)
        {
            // OnValueChange doesn't seem to work
            transform.position = snappedPosition.Value;
            if (IsServer) snappedPosition.Value = Vector3.zero;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void GrabServerRpc(ulong playerNetworkObjectId)
    {
        isGrabbed.Value = true;
    }

    [ServerRpc(RequireOwnership = false)]
    public void UngrabServerRpc(Vector3 snappedPosition)
    {
        isGrabbed.Value = false;
        this.snappedPosition.Value = snappedPosition;
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
}
