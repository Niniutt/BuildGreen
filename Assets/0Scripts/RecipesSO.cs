using System.Collections.Generic;
using UnityEngine;

public enum Type
{
    METAL = 0,
    PLASTIC = 1,
    GLASS = 2,
    DISK = 3,
    BATTERY = 4,
    SCREEN = 5,
    CHIP = 6,
    TV = 7,
    SERVER = 8,
    PHONE = 9,
    PC = 10,
    NULL = 999,
}

public enum OrderStatus
{
    RUNNING = 0,
    FINISHED = 1,
    FAILED = 2,
    NULL = 999,
}

public enum MiniGameType
{
    FILE_TYPE = 0,
    MINIMIZING = 1,
    SEARCH_ALGO = 2,
    NULL = 999,
}

public struct Recipe
{
    public Type output;
    public List<Type> inputs;

    public Recipe(Type output, List<Type> inputs)
    {
        this.output = output;
        this.inputs = inputs;
    }
}

[CreateAssetMenu(fileName = "RecipesSO", menuName = "Scriptable Objects/RecipesSO", order = 1)]
public class RecipesSO : ScriptableObject
{
    public Recipe[] recipes = new Recipe[]
    {
        new Recipe(Type.TV, new List<Type> {Type.SCREEN}),
        new Recipe(Type.SERVER, new List<Type> {Type.CHIP, Type.DISK}),
        new Recipe(Type.PHONE, new List<Type> {Type.SCREEN, Type.CHIP, Type.BATTERY}),
        new Recipe(Type.PC, new List<Type> {Type.SCREEN, Type.CHIP, Type.BATTERY, Type.DISK})
    };

    public List<Type> GetIngredientList(Type type)
    {
        Recipe recipe = recipes[(int)type - (int)Type.TV];
        return recipe.inputs;
    }

    public MiniGameType GetMiniGameType(Type type)
    {
        List<Type> ingredients = GetIngredientList(type);
        // Choose random ingredient
        Type ingredientType = ingredients[Random.Range(0, ingredients.Count)];
        // Choose mini-game type based on the ingredient
        /*switch(ingredientType)
        {
            case Type.SCREEN:
                return MiniGameType.FILE_TYPE;
            case Type.CHIP:
                return MiniGameType.MINIMIZING;
            case Type.DISK:
                return MiniGameType.SEARCH_ALGO;
            default:
                break;
        }*/
        return MiniGameType.MINIMIZING;
    }
}
