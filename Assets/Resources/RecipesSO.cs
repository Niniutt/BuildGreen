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
    EXTINGUISHER = 11,
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
    DARK_MODE = 3,
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
    public Texture2D checkMark;
    public Texture2D failMark;

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

    public string GetIngredientString(Type type)
    {
        string result = "";
        Recipe recipe = recipes[(int)type - (int)Type.TV];
        foreach (Type ingredient in recipe.inputs)
        {
            result += ingredient.ToString()[0] + " ";
        }
        return result;
    }

    // Receives a part type and returns a mini-game
    public MiniGameType GetMiniGameType(Type ingredientType)
    {
        // Choose mini-game type based on the ingredient
        // return MiniGameType.MINIMIZING; // Temporary
        switch (ingredientType)
        {
            case Type.SCREEN:
                return MiniGameType.FILE_TYPE;
            case Type.CHIP:
                return MiniGameType.SEARCH_ALGO;
            case Type.DISK:
                return MiniGameType.MINIMIZING;
            case Type.BATTERY:
                return MiniGameType.DARK_MODE;
            default:
                break;
        }
        return MiniGameType.NULL;
    }
}
