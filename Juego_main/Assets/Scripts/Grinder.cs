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
    // Si esta activo, los ingredientes se echan en el orden de la lista (por ejemplo, primero la leche y luego el cuajo)
    [SerializeField] private bool inOrder;
    // Opcional: lo que se ve dentro por cada ingrediente de la lista (mismo orden que "ingredients"),
    // por ejemplo la leche blanca y las bolitas de cuajo. Si hay rellenos, lo que se echa desaparece
    // de la mano y solo se ve su relleno
    [SerializeField] private List<GameObject> fillStages = new List<GameObject>();

    // Private *****
    private readonly List<Food> _ingredientsInside = new List<Food>();
    private float _grindTimer;
    private bool _isGrinding;

    // MonoBehavior Callbacks *****
    private void Start()
    {
        ShowFillStage();
    }

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

        // Con la hoja en la mano se sirve directo el resultado (por ejemplo, la cuajada amasada)
        if (playerItem && _item && playerItem.TryGetComponent(out Plate plate) &&
            _item.TryGetComponent(out Food done) && plate.TryAddFood(done.GetFoodData()))
        {
            Destroy(_item.gameObject);
            DropItem();
            return;
        }

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
        if (inOrder) return _ingredientsInside.Count < ingredients.Count && ingredients[_ingredientsInside.Count] == foodData;

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

        if (fillStages.Count > 0)
        {
            food.gameObject.SetActive(false);
            ShowFillStage();
            if (IsFull()) _grindTimer = timeToGrind;
            return;
        }

        // Repartir los ingredientes para que se vean todos
        for (int i = 0; i < _ingredientsInside.Count; i++)
        {
            float offset = (i - (_ingredientsInside.Count - 1) / 2f) * ingredientSpacing;
            _ingredientsInside[i].transform.localPosition = new Vector3(offset, 0, 0);
        }

        if (IsFull()) _grindTimer = timeToGrind;
    }

    // Muestra el relleno de cada ingrediente que ya esta dentro (sin importar el orden en que se echo)
    private void ShowFillStage()
    {
        for (int i = 0; i < fillStages.Count; i++)
        {
            if (!fillStages[i]) continue;

            FoodData ingredient = i < ingredients.Count ? ingredients[i] : null;
            fillStages[i].SetActive(ingredient && _ingredientsInside.Exists(food => food && food.GetFoodData() == ingredient));
        }
    }

    private void Grind()
    {
        foreach (Food food in _ingredientsInside)
        {
            if (food) Destroy(food.gameObject);
        }
        _ingredientsInside.Clear();
        ShowFillStage();

        GrabItem(Instantiate(result.prefab).GetComponent<Item>());

        HideProgress();
        _isGrinding = false;
        _onActionComplete?.Invoke();
    }
}
