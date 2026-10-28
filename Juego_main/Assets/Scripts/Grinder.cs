using System;
using System.Collections.Generic;
using UnityEngine;

// Molino: recibe los ingredientes crudos de la lista (por ejemplo, mazorca y queso) y,
// mientras el jugador mantiene la accion, los muele hasta convertirlos en el resultado (masa).
public class Grinder : Tile
{
    [Header("Molino")]
    // Serialized *****
    [SerializeField] private List<FoodData> ingredients = new List<FoodData>();
    [SerializeField] private FoodData result;
    [SerializeField] private float timeToGrind = 3f;
    [SerializeField] private float ingredientSpacing = 0.4f;

    // Private *****
    private readonly List<Food> _ingredientsInside = new List<Food>();
    private float _grindTimer;
    private bool _isGrinding;

    // MonoBehavior Callbacks *****
    private void Update()
    {
        if (!_isGrinding) return;

        _grindTimer -= Time.deltaTime;
        ShowProgress(1f - Mathf.Clamp01(_grindTimer / timeToGrind));
        if (_grindTimer <= 0) Grind();
    }

    // Public Methods *****
    public override void TakeAction(PlayerInteraction owner, Item playerItem, Action onActionComplete)
    {
        _onActionComplete = onActionComplete;

        // Echar un ingrediente al molino
        if (playerItem)
        {
            if (!_item && playerItem.TryGetComponent(out Food food) && Accepts(food.GetFoodData()))
            {
                AddIngredient(food);
                owner.DropItem();
            }
            return;
        }

        // Recoger la masa ya molida
        if (_item)
        {
            if (owner.GrabItem(_item)) DropItem();
            return;
        }

        // Moler (mantener la accion) cuando ya estan todos los ingredientes
        if (IsFull())
        {
            _isGrinding = true;
            owner.StartCutAnimation();
        }
    }

    public override void ActionComplete()
    {
        if (!_isGrinding) return;

        _isGrinding = false;
        _onActionComplete?.Invoke();
    }

    // Private Methods *****
    protected override void TakeAdvanceAction(PlayerInteraction owner)
    {
    }

    private bool Accepts(FoodData foodData)
    {
        int required = 0;
        foreach (FoodData ingredient in ingredients)
        {
            if (ingredient == foodData) required++;
        }

        int inside = 0;
        foreach (Food food in _ingredientsInside)
        {
            if (food.GetFoodData() == foodData) inside++;
        }

        return inside < required;
    }

    private bool IsFull() => _ingredientsInside.Count >= ingredients.Count;

    private void AddIngredient(Food food)
    {
        _ingredientsInside.Add(food);
        food.transform.SetParent(itemAnchor, false);
        food.transform.localPosition = Vector3.zero;

        // Repartir los ingredientes para que se vean todos
        for (int i = 0; i < _ingredientsInside.Count; i++)
        {
            float offset = (i - (_ingredientsInside.Count - 1) / 2f) * ingredientSpacing;
            _ingredientsInside[i].transform.localPosition = new Vector3(offset, 0, 0);
        }

        if (IsFull()) _grindTimer = timeToGrind;
    }

    private void Grind()
    {
        foreach (Food food in _ingredientsInside)
        {
            if (food) Destroy(food.gameObject);
        }
        _ingredientsInside.Clear();

        GrabItem(Instantiate(result.prefab).GetComponent<Item>());

        HideProgress();
        _isGrinding = false;
        _onActionComplete?.Invoke();
    }
}
