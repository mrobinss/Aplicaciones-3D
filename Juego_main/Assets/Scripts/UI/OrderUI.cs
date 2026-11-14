using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OrderUI : MonoBehaviour
{
    // Serialized 
    [SerializeField] private Image timerImage;
    // Icono del plato y de sus ingredientes; se cambian segun la receta del pedido
    [SerializeField] private Image dishIcon;
    [SerializeField] private Image[] ingredientIcons;
    // private *****
    private bool _started;
    private float _orderTime;
    private float _actualOrderTime;
    
    // Monobehavior Callbacks
    

    private void OnEnable()
    {
        RectTransform rect = gameObject.GetComponent<RectTransform>();
        rect.localScale = Vector3.zero;
        LeanTween.scale(rect, Vector3.one,0.5f).setEase(LeanTweenType.easeOutSine);
    }

    private void Update()
    {
        if (!_started) return;

        _actualOrderTime -= Time.deltaTime;
        timerImage.fillAmount = _actualOrderTime / _orderTime;
        
        if (_actualOrderTime <= 0)
        {
            LeanTween.color(gameObject.GetComponent<RectTransform>(), Color.red, 0.5f).setDestroyOnComplete(true);
            _started = false;
        }
        
    }
    
    // Public Methods
    public void SetOrder(RecipeData recipeData)
    {
        _started = true;
        _orderTime = 60;
        _actualOrderTime = _orderTime;
        Dish = recipeData ? recipeData.result : null;
        ShowRecipe(recipeData);
    }

    // Private Methods
    // Cada nivel pide un plato distinto: la tarjeta muestra el de la receta (si tiene imagen)
    private void ShowRecipe(RecipeData recipeData)
    {
        if (!recipeData) return;

        if (dishIcon && recipeData.result && recipeData.result.itemSprite) dishIcon.sprite = recipeData.result.itemSprite;

        if (ingredientIcons == null || recipeData.orderIngredients == null || recipeData.orderIngredients.Count == 0) return;
        for (int i = 0; i < ingredientIcons.Length; i++)
        {
            if (!ingredientIcons[i]) continue;

            FoodData ingredient = i < recipeData.orderIngredients.Count ? recipeData.orderIngredients[i] : null;
            ingredientIcons[i].gameObject.SetActive(ingredient && ingredient.itemSprite);
            if (ingredient && ingredient.itemSprite) ingredientIcons[i].sprite = ingredient.itemSprite;
        }
    }

    // Un pedido esta pendiente mientras su tiempo corre (no vencido ni ya entregado)
    public bool IsPending() => _started;

    // Plato que pide este pedido
    public FoodData Dish { get; private set; }

    public void SuccessAndDestroy()
    {
        _started = false;
        LeanTween.color(gameObject.GetComponent<RectTransform>(), Color.green, 0.5f).setDestroyOnComplete(true);
    }
}
