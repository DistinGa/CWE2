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

    int _PlayerAuthority;
    AI _AI;
    Randomizer _Randomizer;
    GameStates _GState;
    float _TickDuration;
    float _WaitPlayerTurnTime = 300f; //Время ожидания хода игрока (по умолчанию 5 минут)
    bool _f_WaitTimeIsOut;  //Время ожидания хода вышло
    WorldMapGlobe _map;
    [SerializeField] MainWindow _mainWindow;
    [SerializeField] Settings _Settings;

    #region Properties
    public ModProperties GameProperties
    {
        get { return _Settings.ModProperties; }
        private set { _Settings.ModProperties = value; }
    }

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

    public AI AI
    {
        get { return _AI; }
    }

    public Randomizer Randomizer
    {
        get { return _Randomizer; }
    }

    public int PlayerAuthority
    {
        get { return _PlayerAuthority; }
    }
    #endregion

    void Awake ()
    {
        if (GM != null)
        {
            Destroy(gameObject);
            return;
        }

        GM = this;

        //// Тестирование запуска
        //Dictionary<int, int> _dict = new Dictionary<int, int>()
        //{
        //    {1, 100 },
        //    {2, 100 },
        //};
        //Dictionary<int, List<int>> _dict = new Dictionary<int, List<int>>()
        //{
        //    { 1, new List<int>() { 0 } },
        //    { 2, new List<int>() { 1 } },
        //};
        //int[,] _ageMatrix = new int[_Count, _Count]
        //    {
        //        {80, 50, 0, 70, 60, 50, 60 },
        //        {20, 20, 50, 0, 30, 40, 30 },
        //    };

        //nsAI.GeneticAlgorithm _GA = new nsAI.GeneticAlgorithm(_damageMatrix, _targetsValue, _weaponsFireCost, _dictWeapons, _dictSupply);

        //var _tmp = DateTime.Now;
        //Start();
        //print((DateTime.Now - _tmp).Milliseconds);

        _AI = new AI();
        _Randomizer = new Randomizer();
        GameState = GameStates.Initial;
        Assets.SimpleLocalization.LocalizationManager.Read();
        _map = WorldMapGlobe.instance;


    }

    private void Start()
    {
        _TickDuration = GameProperties.TickInterval;
    }

    private void Update()
    {
        if (GameState == GameStates.Regular)
        {
            _TickDuration -= Time.deltaTime;

            if (_TickDuration <= 0)
            {
                _TickDuration = GameProperties.TickInterval;

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

