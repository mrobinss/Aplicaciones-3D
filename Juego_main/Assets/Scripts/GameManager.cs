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
    public event EventHandler OnOrderSuccess;
    
    // Serialized *****
    [SerializeField] private float preGameTime;
    [SerializeField] private float gameTime;
    [SerializeField] private List<RecipeData> recipeDataList = new List<RecipeData>();

    [Header("Resultado del nivel")]
    // Pedidos entregados necesarios para la 1ra, 2da y 3ra estrella
    [SerializeField] private int[] ordersForStars = { 2, 4, 6 };
    [SerializeField] private int starsToWin = 2;
    [SerializeField] private string winScene = "WinScene";
    [SerializeField] private string loseScene = "LoseScene";
    [SerializeField] private float resultDelay = 1.5f;

    // Private *****
    private GameState _gameState;
    private float _stateTimer;
    private float _timeHelper;
    private float _orderTimer;
    private int _coins;
    private int _deliveredOrders;

    private List<RecipeData> _activeRecipes = new List<RecipeData>();

    // MonoBehavior Methods
    private void Start()
    {
        // Idle time
        _stateTimer = preGameTime;
        _coins = 0;
        OnTimeChange?.Invoke(this, gameTime);
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
                    RecipeData newOrder = recipeDataList[0];
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
        StartCoroutine(LoadResultScene(won ? winScene : loseScene));
    }

    public void SuccessOrder()
    {
        if (_gameState == GameState.GameOver) return;

        _deliveredOrders++;
        OnOrderSuccess?.Invoke(this, EventArgs.Empty);
    }

    public int GetDeliveredOrders() => _deliveredOrders;

    // Private Method
    private int GetStars()
    {
        int stars = 0;
        foreach (int required in ordersForStars)
        {
            if (_deliveredOrders >= required) stars++;
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
