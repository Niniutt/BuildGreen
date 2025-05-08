using System.Collections.Generic;
using UnityEngine;



public class Assembly : MonoBehaviour
{
    [SerializeField] private GridManager gridManager;
    [SerializeField] private RecipesSO recipesSO;
    
    private float yOffset = 0f;
    private float craftingTime = 1f;
    private bool inZone = false;

    private void Update()
    {
        // If player inZone and presses F begin craft.
        if (inZone && Input.GetKeyDown(KeyCode.F))
        {
            Assemble();
        }
    }

    private void Assemble()
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
                Debug.Log("Ouiiiiii " + recipe.output.ToString());
                noFound = false;
                output = recipe.output;
                break;
            }
        }
        if (noFound)
        {
            // Display "Invalid recipe" to the player
            Debug.Log("Invalid recipe");
        }
        else
        {
            // If they form a valid recipe, begin craft.
            // = craftingTime timer

            // At the end of this, actually performing the craft in GridManager
            gridManager.Assemble(output);
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
