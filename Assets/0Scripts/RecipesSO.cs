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
}
