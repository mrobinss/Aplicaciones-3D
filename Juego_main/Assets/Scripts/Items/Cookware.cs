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
    // Otras recetas que puede preparar la misma olla (por ejemplo, melao de mora o de breva).
    // La receta se elige con el primer ingrediente que se echa.
    [SerializeField] private List<RecipeData> alternativeRecipes = new List<RecipeData>();
    [SerializeField] private ParticleSystem steamParticles;
    
    [Header("Pot")]
    //TODO: CHANGE TO a specific pot class
    [SerializeField] private Transform potSoup;
    [SerializeField] private Vector2 yMinMaxPotSoup;
    [SerializeField] private ProgressUI _progressUI;
    // Segundos que aguanta la comida lista en el horno antes de quemarse
    [SerializeField] private float burnTime = 10f;
    [SerializeField] private Color burnedColor = new Color(0.18f, 0.1f, 0.06f, 1f);
    // Al quemarse suena una alerta, se ve quemado un momento y la olla se vacia sola
    [SerializeField] private AudioClip burnedAlert;
    [SerializeField] private float discardBurnedDelay = 1.5f;

    // Private *****
    private CookwareState _cookwareState;
    private List<FoodData> _foodDataList = new List<FoodData>();
    private float _cookwareTimer;
    private Renderer _soupRenderer;
    private RecipeData _baseRecipe;

    // MonoBehavior Callbacks *****

    private void Start()
    {
        // localPosition: con position (mundo) el liquido quedaba bajo el piso cuando la olla esta en alto
        _soupRenderer = potSoup.GetComponent<Renderer>();
        UpdateSoup();
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
                _cookwareState = CookwareState.Burned;
                SetBurnedLook(true);
                steamParticles.Stop();
                if (burnedAlert) AudioManager.CreateSFX(burnedAlert, 0.8f);
                if (GameManager.Instance) GameManager.Instance.FoodBurned();
                StartCoroutine(DiscardBurned());
                break;
        }
    }
    
    // Private MEthods *****
    // yMinMaxPotSoup: x = altura con el primer ingrediente, y = cuanto sube hasta llenarse.
    // Vacia, la olla no muestra liquido (la olla de barro se ve por dentro)
    private void UpdateSoup()
    {
        float percentage = (float)_foodDataList.Count / maxCapacity;
        float level = yMinMaxPotSoup.x + percentage * yMinMaxPotSoup.y;
        potSoup.localPosition = new Vector3(potSoup.localPosition.x, level, potSoup.localPosition.z);
        if (_soupRenderer) _soupRenderer.enabled = _foodDataList.Count > 0;
    }


    // Con la olla vacia, el primer ingrediente decide cual receta se prepara
    private void ChooseRecipe(FoodData foodData)
    {
        if (_foodDataList.Count > 0 || alternativeRecipes.Count == 0) return;

        if (!_baseRecipe) _baseRecipe = recipe;
        List<RecipeData> options = new List<RecipeData> { _baseRecipe };
        options.AddRange(alternativeRecipes);
        foreach (RecipeData option in options)
        {
            if (!option || !option.materials.Exists(material => material.food == foodData)) continue;

            recipe = option;
            if (option.cookwareMaterial && _soupRenderer) _soupRenderer.sharedMaterial = option.cookwareMaterial;
            return;
        }
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
        ChooseRecipe(foodData);
        if (!RecipeAccepts(foodData)) return false;

        _foodDataList.Add(foodData);
        UpdateSoup();
        
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

    private IEnumerator DiscardBurned()
    {
        yield return new WaitForSeconds(discardBurnedDelay);

        if (_cookwareState == CookwareState.Burned) TryEmpty();
    }

    // Vacia la olla (por ejemplo, comida quemada que se bota en la composta)
    public bool TryEmpty()
    {
        if (_foodDataList.Count == 0 && _cookwareState == CookwareState.Empty) return false;

        _foodDataList.Clear();
        _cookwareState = CookwareState.Empty;
        steamParticles.Stop();
        SetBurnedLook(false);
        UpdateSoup();
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

    // Lo que se serviria de la olla, sin sacarlo todavia
    public bool TryPeekDish(out FoodData foodCooked)
    {
        bool ready = _cookwareState == CookwareState.Cooked || _cookwareState == CookwareState.CookedCooking;
        foodCooked = ready ? recipe.result : null;
        return ready;
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
            UpdateSoup();
            return true;
        }

        foodCooked = null;
        return false;
    }
}
