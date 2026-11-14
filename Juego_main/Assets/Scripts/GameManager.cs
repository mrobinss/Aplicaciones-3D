using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    #region Singletone

    public static GameManager Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    #endregion
    
    // Public
    public enum GameState
    {
        PreGame,
        InGame,
        GameOver
    }
    
    public event EventHandler<int> OnCurrencyChange;
    public event EventHandler<float> OnTimeChange;
    public event EventHandler<RecipeData> OnOrderCreate;
    public event EventHandler<FoodData> OnOrderSuccess;
    
    // Serialized *****
    [SerializeField] private float preGameTime;
    [SerializeField] private float gameTime;
    [SerializeField] private List<RecipeData> recipeDataList = new List<RecipeData>();

    [Header("Resultado del nivel")]
    // Numero del nivel en el orden del mapa (ganarlo desbloquea el siguiente)
    [SerializeField] private int levelNumber = 1;
    // Pedidos entregados necesarios para la 1ra, 2da y 3ra estrella
    [SerializeField] private int[] ordersForStars = { 2, 4, 6 };
    [SerializeField] private int starsToWin = 2;
    [SerializeField] private string winScene = "WinScene";
    [SerializeField] private string loseScene = "LoseScene";
    [SerializeField] private float resultDelay = 1.5f;

    [Header("Puntos")]
    // Las estrellas se ganan con puntos: cada pedido de ordersForStars equivale a pointsPerOrder
    [SerializeField] private int pointsPerOrder = 10;
    [SerializeField] private int burnPenalty = 5;
    [SerializeField] private int compostLeafBonus = 2;

    // Private *****
    private GameState _gameState;
    private float _stateTimer;
    private float _timeHelper;
    private float _orderTimer;
    private int _points;
    private int _deliveredOrders;

    private List<RecipeData> _activeRecipes = new List<RecipeData>();
    // Pedidos que faltan de la ronda actual: cada ronda trae una vez cada receta en orden al azar
    private readonly List<RecipeData> _orderBag = new List<RecipeData>();

    // MonoBehavior Methods
    private void Start()
    {
        LevelProgress.CurrentLevel = levelNumber;

        // Idle time
        _stateTimer = preGameTime;
        _points = 0;
        OnTimeChange?.Invoke(this, gameTime);
        OnCurrencyChange?.Invoke(this, _points);
    }

    private void Update()
    {
        _stateTimer -= Time.deltaTime;
        _timeHelper -= Time.deltaTime;
        _orderTimer -= Time.deltaTime;

        switch (_gameState)
        {
            case GameState.PreGame:
                if (_stateTimer <= 0)
                {
                    _stateTimer = gameTime;
                    _gameState = GameState.InGame;
                }

                break;
            case GameState.InGame:
                if (_timeHelper <= 0)
                {
                    OnTimeChange?.Invoke(this, _stateTimer);
                    _timeHelper = 1;
                }

                if (_orderTimer <= 0 && recipeDataList.Count > 0)
                {
                    // Si el nivel tiene varios platos, se van alternando (sin repetir mucho el mismo)
                    RecipeData newOrder = NextOrder();
                    _activeRecipes.Add(newOrder);
                    OnOrderCreate?.Invoke(this,newOrder);
                    _orderTimer = 35;
                }
                if (_stateTimer <= 0)
                {
                    SetGameOver();
                }
                break;
        }
        

    }
    
    // public Methods *****
    public void SetGameOver()
    {
        if (_gameState == GameState.GameOver) return;

        _gameState = GameState.GameOver;
        OnTimeChange?.Invoke(this, 0);

        int stars = GetStars();
        bool won = stars >= starsToWin;
        LevelResult.Set(_deliveredOrders, stars, won);
        if (won) LevelProgress.Unlock(levelNumber + 1);
        StartCoroutine(LoadResultScene(won ? winScene : loseScene));
    }

    public void SuccessOrder(FoodData dish)
    {
        if (_gameState == GameState.GameOver) return;

        _deliveredOrders++;
        AddPoints(pointsPerOrder);
        OnOrderSuccess?.Invoke(this, dish);
    }

    // Dejar quemar la comida en el horno resta puntos
    public void FoodBurned() => AddPoints(-burnPenalty);

    // Compostar hojas sucias da puntos extra (por cada hoja)
    public void LeavesComposted(int leaves) => AddPoints(compostLeafBonus * leaves);

    public int GetPoints() => _points;

    public int GetDeliveredOrders() => _deliveredOrders;

    // El plato entregado es el resultado de alguna receta que se pide en este nivel
    public bool IsOrderDish(FoodData dish)
    {
        if (!dish) return false;
        foreach (RecipeData recipe in recipeDataList)
        {
            if (recipe && recipe.result == dish) return true;
        }
        return false;
    }

    // Private Method
    private RecipeData NextOrder()
    {
        if (_orderBag.Count == 0)
        {
            _orderBag.AddRange(recipeDataList);
            // Que la ronda nueva no empiece con el mismo plato con que termino la anterior
            RecipeData last = _activeRecipes.Count > 0 ? _activeRecipes[_activeRecipes.Count - 1] : null;
            for (int i = _orderBag.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                (_orderBag[i], _orderBag[j]) = (_orderBag[j], _orderBag[i]);
            }
            if (_orderBag.Count > 1 && _orderBag[0] == last) (_orderBag[0], _orderBag[1]) = (_orderBag[1], _orderBag[0]);
        }

        RecipeData next = _orderBag[0];
        _orderBag.RemoveAt(0);
        return next;
    }

    private void AddPoints(int amount)
    {
        if (_gameState == GameState.GameOver) return;

        _points = Mathf.Max(0, _points + amount);
        OnCurrencyChange?.Invoke(this, _points);
    }

    private int GetStars()
    {
        int stars = 0;
        foreach (int required in ordersForStars)
        {
            if (_points >= required * pointsPerOrder) stars++;
        }
        return stars;
    }

    // Pequena pausa para que se vea el reloj en 00:00 y luego el resultado aparece como pop-up
    // encima del nivel (la escena de resultado pausa el juego que queda detras)
    private IEnumerator LoadResultScene(string sceneName)
    {
        yield return new WaitForSecondsRealtime(resultDelay);
        SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
    }

}
