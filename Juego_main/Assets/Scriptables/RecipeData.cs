using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct FoodAmount
{
    public FoodData food;
    [Range(1,999)]
    public int amount;
}

[CreateAssetMenu (menuName = "New Recipe")]
public class RecipeData : ScriptableObject
{
    public List<FoodAmount> materials;
    public FoodData result;
    [Range(0,999)]
    public float cookingTime;
    // Opcional: ingredientes que se muestran en la tarjeta del pedido (si esta vacio se deja la del prefab)
    public List<FoodData> orderIngredients = new List<FoodData>();
    // Opcional: color del contenido de la olla mientras se prepara esta receta
    public Material cookwareMaterial;
}
