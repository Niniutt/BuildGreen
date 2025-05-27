using Unity.Netcode;
using UnityEngine;

public class Check : NetworkBehaviour
{
    [SerializeField] private LevelManager levelManager;
    private float probability;

    private void Start()
    {
        probability = LevelManager.MINI_GAME_PROBABILITY;
    }

    private void OnTriggerEnter(Collider collision)
    {
        var grabbable = collision.GetComponent<Grabbable>();
        if (grabbable != null && grabbable.NetworkObject != null && grabbable.NetworkObject.IsSpawned)
        {
            CheckServerRpc(grabbable.NetworkObject.NetworkObjectId);
        }
    }


    [ServerRpc (RequireOwnership = false)]
    private void CheckServerRpc(ulong networkObjectId)
    {
        NetworkObject netObj = NetworkManager.Singleton.SpawnManager.SpawnedObjects[networkObjectId];
        if (netObj == null) return;
        GameObject go = netObj.gameObject;

        Grabbable grab = go.GetComponent<Grabbable>();
        if (grab.isGrabbed.Value == true || grab.isChecked.Value == true) return;
        bool check = Random.Range(0f, 1f) > probability;
        grab.UpdateCheckClientRpc(networkObjectId, check);
        // Call grabbable to update mini-game
        if (!check) grab.UpdateMiniGameType(grab.type.Value);
    }
}
