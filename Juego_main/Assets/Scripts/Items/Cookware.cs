using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cookware : Item
{
    // Public *****
    public enum CookwareState
    {
        Empty, // Applies if its not full
        RawValidRecipe, 
        Cooking, // Change to this state depends on Stove
        Cooked,
        CookedCooking,
        Burned
    }
    // Public *****
    public EventHandler<Cookware> OnFullAndValidRecipe;
    
    // Serialized
    [Header("Cookware")]
    [SerializeField] private int maxCapacity = 1;
    // TODO: Change this to a list
    [SerializeField] private RecipeData recipe;
    [SerializeField] private ParticleSystem steamParticles;
    
    [Header("Pot")]
    //TODO: CHANGE TO a specific pot class
    [SerializeField] private Transform potSoup;
    [SerializeField] private Vector2 yMinMaxPotSoup;
    [SerializeField] private ProgressUI _progressUI;
    // Segundos que aguanta la comida lista en el horno antes de quemarse
    [SerializeField] private float burnTime = 10f;
    [SerializeField] private Color burnedColor = new Color(0.18f, 0.1f, 0.06f, 1f);

    // Private *****
    private CookwareState _cookwareState;
    private List<FoodData> _foodDataList = new List<FoodData>();
    private float _cookwareTimer;
    private Renderer _soupRenderer;

    // MonoBehavior Callbacks *****

    private void Start()
    {
        // localPosition: con position (mundo) el liquido quedaba bajo el piso cuando la olla esta en alto
        potSoup.localPosition = new Vector3(potSoup.localPosition.x, yMinMaxPotSoup.x, potSoup.localPosition.z);
        _soupRenderer = potSoup.GetComponent<Renderer>();
    }

    // Private Methods *****
    private void Update()
    {
        if (!IsCooking()) return;
        
        _cookwareTimer -= Time.deltaTime;

        if (_cookwareState == CookwareState.Cooking)
        {
            OnItemProgressChange?.Invoke(this, _cookwareTimer/ recipe.cookingTime);
        }
        
        if (!(_cookwareTimer <= 0)) return;
        
        switch (_cookwareState)
        {
            case CookwareState.Cooking:
                steamParticles.Play();
                _cookwareState = CookwareState.CookedCooking;
                _cookwareTimer = burnTime;
                break;
            case CookwareState.CookedCooking:
                Debug.Log("your comnida is burn");
                _cookwareState = CookwareState.Burned;
                SetBurnedLook(true);
                break;
        }
    }
    
    // Private MEthods *****
    private float GetSoupLevel()
    {
        float percentage = (float)_foodDataList.Count / maxCapacity;
        float limit = yMinMaxPotSoup.x + yMinMaxPotSoup.y;

        return percentage * limit;
    }


    // Solo acepta ingredientes de la receta y hasta la cantidad que pide cada uno
    private bool RecipeAccepts(FoodData foodData)
    {
        if (!recipe || recipe.materials == null || recipe.materials.Count == 0) return true;

        foreach (FoodAmount material in recipe.materials)
        {
            if (material.food != foodData) continue;

            int current = 0;
            foreach (FoodData food in _foodDataList)
            {
                if (food == foodData) current++;
            }
            return current < material.amount;
        }
        return false;
    }

    // Public Methods *****
    public bool AddFood(FoodData foodData)
    {
        if (_foodDataList.Count >= maxCapacity || foodData.foodType != FoodType.Processed) return false;
        if (!RecipeAccepts(foodData)) return false;

        _foodDataList.Add(foodData);
        potSoup.localPosition = new Vector3(potSoup.localPosition.x, GetSoupLevel(), potSoup.localPosition.z);
        
        //CookwareFull
        if (_foodDataList.Count >= maxCapacity)
        {
            // TODO: Check if is a valid recipe
            _cookwareState = CookwareState.RawValidRecipe;
            OnFullAndValidRecipe?.Invoke(this, this);
        }
        
        return true;
    }

    public void StartCook()
    {
        if(_cookwareState == CookwareState.RawValidRecipe)
        {
            _cookwareState = CookwareState.Cooking;
            _cookwareTimer = recipe.cookingTime;
            
            ProgressUI progressUI = Instantiate(_progressUI);
            progressUI.transform.SetParent(GameObject.FindGameObjectWithTag("CanvasInteraction").transform);
            progressUI.Set(this);
            
        }
        else if (_cookwareState == CookwareState.Cooked)
        {
            _cookwareState = CookwareState.CookedCooking;
            _cookwareTimer = burnTime;
        }
    }

    public void StopCook()
    {
        if(_cookwareState == CookwareState.Cooking) _cookwareState = CookwareState.RawValidRecipe;
        else if (_cookwareState == CookwareState.CookedCooking) _cookwareState = CookwareState.Cooked;
    }

    public bool CanBeCooked() =>
        (_cookwareState == CookwareState.RawValidRecipe || _cookwareState == CookwareState.Cooked);
    
    private bool IsCooking() => (_cookwareState == CookwareState.Cooking || _cookwareState == CookwareState.CookedCooking);

    // Vacia la olla (por ejemplo, comida quemada que se bota en la composta)
    public bool TryEmpty()
    {
        if (_foodDataList.Count == 0 && _cookwareState == CookwareState.Empty) return false;

        _foodDataList.Clear();
        _cookwareState = CookwareState.Empty;
        steamParticles.Stop();
        SetBurnedLook(false);
        potSoup.localPosition = new Vector3(potSoup.localPosition.x, GetSoupLevel(), potSoup.localPosition.z);
        return true;
    }

    // La comida quemada se ve oscura para que el jugador sepa que ya no se puede servir
    private void SetBurnedLook(bool burned)
    {
        if (!_soupRenderer) return;

        if (!burned)
        {
            _soupRenderer.SetPropertyBlock(null);
            return;
        }
        MaterialPropertyBlock block = new MaterialPropertyBlock();
        block.SetColor("_BaseColor", burnedColor);
        _soupRenderer.SetPropertyBlock(block);
    }

    public bool TryGetDish(out FoodData foodCooked)
    {
        // Tambien se puede servir con la olla todavia en el horno (antes de que se queme)
        if (_cookwareState == CookwareState.Cooked || _cookwareState == CookwareState.CookedCooking)
        {
            foodCooked = recipe.result;
            _foodDataList.Clear();
            _cookwareState = CookwareState.Empty;
            steamParticles.Stop();
            potSoup.localPosition = new Vector3(potSoup.localPosition.x, GetSoupLevel(), potSoup.localPosition.z);
            return true;
        }

        foodCooked = null;
        return false;
    }
}
