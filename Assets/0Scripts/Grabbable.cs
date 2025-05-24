using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Grabbable : NetworkBehaviour
{
    public NetworkVariable<Type> type;
    public NetworkVariable<bool> isGrabbed = new(false);
    public NetworkVariable<bool> isChecked = new(false);
    public NetworkVariable<bool> isDeliveryReady = new(false);

    public NetworkVariable<Type> miniGameBase = new(Type.NULL);
    public NetworkVariable<MiniGameType> miniGameType = new(MiniGameType.NULL);

    private NetworkVariable<Vector3> grabberPosition = new(Vector3.zero, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<Quaternion> grabberRotation = new(Quaternion.identity, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public NetworkVariable<Vector3> snappedPosition = new(Vector3.zero);

    private RecipesSO recipesSO;

    public override void OnNetworkSpawn()
    {
        recipesSO = Resources.Load<RecipesSO>("RecipesSO");
        if (recipesSO == null)
        {
            Debug.LogError("RecipesSO not found in Resources folder.");
            return;
        }
    }

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

    public void UpdateMiniGameType(Type type)
    {
        List<Type> ingredients = recipesSO.GetIngredientList(type);
        // Choose random ingredient
        Type ingredientType = ingredients[Random.Range(0, ingredients.Count)];
        MiniGameType mgt = recipesSO.GetMiniGameType(ingredientType);
        miniGameBase.Value = ingredientType;
        miniGameType.Value = mgt;
    }



    [ClientRpc]
    public void UpdateCheckClientRpc(ulong networkObjectId, bool check)
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
            check ? recipesSO.checkMark : recipesSO.failMark,
            new Rect(0, 0, recipesSO.checkMark.width, recipesSO.checkMark.height),
            new Vector2(0.5f, 0.5f)
        );
    }
}
