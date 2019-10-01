using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using nsEventSystem;
using nsWorld;
using nsMilitary;
using ModEditor;
using nsAI;
using WPM;
using CWE2UI;

public class GameManager : MonoBehaviour {
    public static GameManager GM;

    AI _AI;
    GameStates _GState;
    float _TickDuration;
    float _WaitPlayerTurnTime = 300f; //Время ожидания хода игрока (по умолчанию 5 минут)
    bool _f_WaitTimeIsOut;  //Время оидания хода вышло
    WorldMapGlobe _map;
    [SerializeField] MainWindow _mainWindow;

    #region Properties
    public GameStates GameState
    {
        get { return _GState; }
        private set { _GState = value; }
    }

    public WorldMapGlobe Map
    {
        get { return _map; }
    }

    public MainWindow MainWindow
    {
        get { return _mainWindow; }
    }
    #endregion

    public AI AI
    {
        get { return _AI; }
    }

    void Awake ()
    {
        if (GM != null)
        {
            Destroy(gameObject);
            return;
        }

        GM = this;

        _AI = new AI();
        GameState = GameStates.Initial;
        Assets.SimpleLocalization.LocalizationManager.Read();
        _map = WorldMapGlobe.instance;

        //// Тестирование GA
        //const int _targetsCount = 7, _weaponsCount = 5;
        //int[] _targetsValue = new int[_targetsCount] {100,80,110,50,60,70,90};
        //int[] _weaponsFireCost = new int[_weaponsCount] { 100, 100, 100, 100, 100 };
        //Dictionary<int, int> _dictSupply = new Dictionary<int, int>()
        //{
        //    {1, 100 },
        //    {2, 100 },
        //    {3, 100 },
        //    {4, 100 }
        //};
        //Dictionary<int, List<int>> _dictWeapons = new Dictionary<int, List<int>>()
        //{
        //    { 1, new List<int>() { 0 } },
        //    { 2, new List<int>() { 1 } },
        //    { 3, new List<int>() { 2 } },
        //    { 4, new List<int>() { 3 } },
        //    { 5, new List<int>() { 4 } }
        //};
        //int[,] _damageMatrix = new int[_weaponsCount, _targetsCount]
        //    {
        //        {80, 50, 0, 70, 60, 50, 60 },
        //        {70, 40, 80, 0, 40, 30, 50 },
        //        {60, 50, 70, 30, 40, 50, 60 },
        //        {20, 20, 50, 0, 30, 40, 30 },
        //        {40, 30, 50, 10, 20, 20, 40 }
        //    };

        //nsAI.GeneticAlgorithm _GA = new nsAI.GeneticAlgorithm(_damageMatrix, _targetsValue, _weaponsFireCost, _dictWeapons, _dictSupply);

        //var _tmp = DateTime.Now;
        //var _GAres = _GA.GetSolution(1000, false);
        //print((DateTime.Now - _tmp).Milliseconds);
    }

    private void Start()
    {
        _TickDuration = ModProperties.Instance.TickInterval;
    }

    private void Update()
    {
        if (GameState == GameStates.Regular)
        {
            _TickDuration -= Time.deltaTime;

            if (_TickDuration <= 0)
            {
                _TickDuration = ModProperties.Instance.TickInterval;

                StartCoroutine(GameTurn());
            }
        }
    }

    /// <summary>
    /// Ход игры. Все регионы выполняют свой ход поочереди. Выполняется ожидание хода игрока.
    /// </summary>
    IEnumerator GameTurn()
    {
        GameState = GameStates.WaitPlayerTurn;
        GameEventSystem.InvokeEvents(GameEventSystem.MyEventsTypes.TurnActions);

        foreach (var item in World.TheWorld.Regions)
        {
            item.Value.MakeTurnRegion();
            yield return new WaitForSeconds(1);
            StartCoroutine(WaitTurnPlayer());   //Запуск счётчика ожидания хода
            yield return new WaitUntil(() => item.Value.WaitTurnRegion() || _f_WaitTimeIsOut);
        }

        GameEventSystem.InvokeEvents(GameEventSystem.MyEventsTypes.TurnResults);
        GameState = GameStates.Regular;
    }

    /// <summary>
    /// Счётчик ожидания хода игрока
    /// </summary>
    /// <returns></returns>
    IEnumerator WaitTurnPlayer()
    {
        _f_WaitTimeIsOut = false;
        yield return new WaitForSeconds(_WaitPlayerTurnTime);
        _f_WaitTimeIsOut = true;
    }

    public void StartGame(bool Load)
    {
        ModProperties.CreateModProperties();
        World.CreateWorld();
        MilitaryManager.CreateMilitaryManager(new MilitaryManager_Ds());

        GameState = GameStates.Regular;
    }

    #region Свойства
    public bool Pause
    {
        get { return GameState == GameStates.Paused; }

        set
        {
            if(value)
                GameState = GameStates.Paused;
            else
                GameState = GameStates.Regular;
        }
    }
    #endregion
}


public enum GameStates
{
    Initial,
    Saving,
    Loading,
    Regular,
    Paused,
    WaitPlayerTurn
}

