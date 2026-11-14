using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plate : Item
{
    // Serialized
    [SerializeField] private Transform itemAnchor;
    // Platos que se arman sobre la hoja juntando varias cosas (por ejemplo, cuajada + melao).
    // Cada receta lista sus partes en "materials" y el plato terminado en "result".
    [SerializeField] private List<RecipeData> platingRecipes = new List<RecipeData>();
    // Private *****
    private GameObject _food;
    private FoodData _foodData;
    private readonly List<FoodData> _contents = new List<FoodData>();

    // Public Methods
    public void PlateUpSoup(FoodData foodData) => TryAddFood(foodData);

    // Indica si la hoja puede recibir este alimento: si esta vacia, cualquier comida lista o
    // cualquier parte de un plato armado; si ya tiene algo, solo lo que completa una receta.
    public bool CanAddFood(FoodData foodData)
    {
        if (!foodData) return false;
        if (_contents.Count == 0) return foodData.foodType == FoodType.Cooked || IsPlatingPart(foodData);

        return FindPlatingRecipe(foodData, out _) != null;
    }

    public bool TryAddFood(FoodData foodData)
    {
        if (!CanAddFood(foodData)) return false;

        RecipeData recipe = _contents.Count > 0 ? FindPlatingRecipe(foodData, out _) : null;
        _contents.Add(foodData);

        // Si con esto la receta queda completa se muestra el plato terminado
        bool finished = recipe && _contents.Count == CountParts(recipe);
        ShowFood(finished ? recipe.result : foodData, replace: finished || _contents.Count == 1);
        return true;
    }

    public void Unplate()
    {
        if (!_food) return;

        foreach (Transform child in itemAnchor) Destroy(child.gameObject);
        _food = null;
        _foodData = null;
        _contents.Clear();
    }

    public bool IsCookedFood() => _foodData != null;

    // Lo que se ve encima de la hoja (el plato terminado cuando la receta se completa)
    public FoodData GetDish() => _foodData;

    // Private Methods
    private void ShowFood(FoodData foodData, bool replace)
    {
        if (replace)
        {
            foreach (Transform child in itemAnchor) Destroy(child.gameObject);
        }

        _foodData = foodData;
        _food = Instantiate(foodData.prefab, itemAnchor, false);
        _food.transform.localPosition = Vector3.zero;
        // Sobre la hoja solo se muestra: no debe comportarse como un ingrediente suelto
        foreach (Collider foodCollider in _food.GetComponentsInChildren<Collider>()) foodCollider.enabled = false;
    }

    private bool IsPlatingPart(FoodData foodData)
    {
        foreach (RecipeData recipe in platingRecipes)
        {
            if (!recipe) continue;
            foreach (FoodAmount material in recipe.materials)
            {
                if (material.food == foodData) return true;
            }
        }
        return false;
    }

    // Busca una receta que contenga lo que ya hay en la hoja mas el nuevo alimento
    private RecipeData FindPlatingRecipe(FoodData newFood, out bool complete)
    {
        complete = false;
        foreach (RecipeData recipe in platingRecipes)
        {
            if (!recipe || !Fits(recipe, newFood)) continue;

            complete = _contents.Count + 1 == CountParts(recipe);
            return recipe;
        }
        return null;
    }

    private bool Fits(RecipeData recipe, FoodData newFood)
    {
        foreach (FoodAmount material in recipe.materials)
        {
            int wanted = material.amount;
            int current = newFood == material.food ? 1 : 0;
            foreach (FoodData food in _contents)
            {
                if (food == material.food) current++;
            }
            if (current > wanted) return false;
        }

        // Todo lo que hay en la hoja tiene que ser parte de la receta
        foreach (FoodData food in _contents)
        {
            if (!recipe.materials.Exists(material => material.food == food)) return false;
        }
        return recipe.materials.Exists(material => material.food == newFood);
    }

    private static int CountParts(RecipeData recipe)
    {
        int parts = 0;
        foreach (FoodAmount material in recipe.materials) parts += material.amount;
        return parts;
    }
}
