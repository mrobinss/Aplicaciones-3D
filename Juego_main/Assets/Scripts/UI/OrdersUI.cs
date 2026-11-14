using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrdersUI : MonoBehaviour
{
    // Serialized
    [SerializeField] private OrderUI orderUiPrefab;

    private List<OrderUI> _activeOrders = new List<OrderUI>();

    // Pedidos en pantalla del nivel actual (la ventana de entrega pregunta aqui)
    public static OrdersUI Instance { get; private set; }

    // MonoBehaviour Callbacks
    private void OnEnable()
    {
        Instance = this;
        GameManager.Instance.OnOrderCreate += GameManager_OnOrderCreate;
        GameManager.Instance.OnOrderSuccess += GameManager_OnOrderSuccess;
    }

    private void OnDisable()
    {
        if (Instance == this) Instance = null;
        GameManager.Instance.OnOrderCreate -= GameManager_OnOrderCreate;
        GameManager.Instance.OnOrderSuccess -= GameManager_OnOrderSuccess;
    }
    
    // Public Methods
    // Hay un pedido pendiente (no vencido ni entregado) que pide este plato
    public bool WantsDish(FoodData dish) => _activeOrders.Exists(order => order && order.IsPending() && order.Dish == dish);

    // Private Methods
    private void GameManager_OnOrderCreate(object sender, RecipeData recipeData)
    {
        OrderUI newOrder = Instantiate(orderUiPrefab, transform);
        newOrder.SetOrder(recipeData);
        _activeOrders.Add(newOrder);
    }
    
    private void GameManager_OnOrderSuccess(object sender, FoodData dish)
    {
        // Quitar pedidos ya destruidos o vencidos; antes se usaba siempre el indice 0 aunque ese
        // pedido ya no existiera, lo que lanzaba un error y dejaba trabada la entrega
        _activeOrders.RemoveAll(order => !order || !order.IsPending());
        if (_activeOrders.Count <= 0) return;

        // Se completa el pedido mas antiguo de ese mismo plato (o el mas antiguo si ninguno coincide)
        OrderUI completedOrder = _activeOrders.Find(order => order.Dish == dish) ?? _activeOrders[0];
        _activeOrders.Remove(completedOrder);
        completedOrder.SuccessAndDestroy();
    }
}
