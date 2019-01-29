using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using nsEventSystem;
using nsWorld;
using nsMilitary;
using ModEditor;

public class GameManager : MonoBehaviour {
    public static GameManager GM;

    GameStates _GState;
    float _TickDuration;
    float _WaitPlayerTurnTime = 300f; //Время ожидания хода игрока (по умолчанию 5 минут)
    bool _f_WaitTimeIsOut;  //Время оидания хода вышло

    public GameStates GameState
    {
        get { return _GState; }
        private set { _GState = value; }
    }

    void Awake ()
    {
        if (GM != null)
        {
            Destroy(gameObject);
            return;
        }

        GM = this;

        GameState = GameStates.Initial;
        Assets.SimpleLocalization.LocalizationManager.Read();
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
        MilitaryManager.CreateMilitaryManager();

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

