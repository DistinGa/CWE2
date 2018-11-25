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

                GameEventSystem.InvokeEvents(GameEventSystem.MyEventsTypes.TurnActions);
                GameEventSystem.InvokeEvents(GameEventSystem.MyEventsTypes.TurnResults);
            }
        }
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
    Paused
}

