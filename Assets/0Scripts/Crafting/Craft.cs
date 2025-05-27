using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;



public class Craft : NetworkBehaviour
{
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private GridManager gridManager;
    [SerializeField] private RecipesSO recipesSO;
    
    private bool inZone = false;

    private void Update()
    {
        // If player inZone and presses F begin craft.
        if (inZone && Input.GetKeyDown(KeyCode.F))
        {
            BuildGreenUtils.ShowFeedback("Attempting craft...");
            AssembleServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void AssembleServerRpc()
    {
        // Check items on the assembly table
        List<Type> list = gridManager.GetAssemblyCandidates();

        // Check recipe
        bool noFound = true;
        Type output = Type.NULL;
        for (int i = 0; i < recipesSO.recipes.Length; i++)
        {
            Recipe recipe = recipesSO.recipes[i];
            if (ListsEqualIgnoreOrder(recipe.inputs, list))
            {
                // Valid recipe
                noFound = false;
                output = recipe.output;
                break;
            }
        }
        if (noFound)
        {
            Debug.Log("Invalid recipe!");
            levelManager.LogServerRpc(0, 0, 0, 1);
        }
        else
        {
            gridManager.Assemble(output);
            levelManager.LogServerRpc(0, 0, 1);
        }
    }

    private bool ListsEqualIgnoreOrder<T>(List<T> a, List<T> b)
    {
        return new HashSet<T>(a).SetEquals(b);
    }

    private void OnTriggerStay(Collider other)
    {
        inZone = true;
    }

    private void OnTriggerExit(Collider other)
    {
        inZone = false;
    }
}
