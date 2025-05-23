using Unity.Netcode;
using UnityEngine;

public class Check : NetworkBehaviour
{
    [SerializeField] private LevelManager levelManager;
    private float probability;

    [SerializeField] private Texture2D checkMark;
    [SerializeField] private Texture2D failMark;

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


    [ServerRpc]
    private void CheckServerRpc(ulong networkObjectId)
    {
        NetworkObject netObj = NetworkManager.Singleton.SpawnManager.SpawnedObjects[networkObjectId];
        if (netObj == null) return;
        GameObject go = netObj.gameObject;

        Grabbable grab = go.GetComponent<Grabbable>();
        if (grab.isGrabbed.Value == true || grab.isChecked.Value == true) return;
        bool check = Random.Range(0f, 1f) > probability;
        grab.isChecked.Value = true;
        UpdateCheckClientRpc(networkObjectId, check);
        // Call LevelManager and launch mini-game
    }

    [ClientRpc]
    private void UpdateCheckClientRpc(ulong networkObjectId, bool check)
    {
        NetworkObject netObj = NetworkManager.Singleton.SpawnManager.SpawnedObjects[networkObjectId];
        if (netObj == null) return;
        GameObject go = netObj.gameObject;
        SpriteRenderer spriteRenderer = go.GetComponentInChildren<SpriteRenderer>();
        if (!spriteRenderer) return;

        if (check) spriteRenderer.sprite = NewSprite();
        else spriteRenderer.sprite = NewSprite(false);
    }

    private Sprite NewSprite(bool check = true)
    {
        return Sprite.Create(
            check? checkMark : failMark,
            new Rect(0, 0, checkMark.width, checkMark.height),
            new Vector2(0.5f, 0.5f)
        );
    }
}
