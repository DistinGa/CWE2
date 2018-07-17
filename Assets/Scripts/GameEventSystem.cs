using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class GameEventSystem
{
    private static GameEventSystem _Instance;

    private Action _TurnEvents, _EndMonthEvents, _NewMonthEvents, _EndYearEvents, _NewYearEvents;

    public static GameEventSystem Instance
    {
        get
        {
            if(_Instance == null)
                _Instance = new GameEventSystem();

            return _Instance;
        }
    }

    #region Common
    public void SubscribeOnTurn(Action dlg, bool Subscribe = true)
    {
        if(Subscribe)
            _TurnEvents += dlg;
        else
            _TurnEvents -= dlg;
    }

    public void SubscribeOnEndMonth(Action dlg, bool Subscribe = true)
    {
        if (Subscribe)
            _EndMonthEvents += dlg;
        else
            _EndMonthEvents -= dlg;
    }

    public void SubscribeOnNewMonth(Action dlg, bool Subscribe = true)
    {
        if (Subscribe)
            _NewMonthEvents += dlg;
        else
            _NewMonthEvents -= dlg;
    }

    public void SubscribeOnEndYear(Action dlg, bool Subscribe = true)
    {
        if (Subscribe)
            _EndYearEvents += dlg;
        else
            _EndYearEvents -= dlg;
    }

    public void SubscribeOnNewYear(Action dlg, bool Subscribe = true)
    {
        if (Subscribe)
            _NewYearEvents += dlg;
        else
            _NewYearEvents -= dlg;
    }

    public void InvokeTurnEvents()
    {
        if(_TurnEvents != null)
            _TurnEvents();
    }

    public void InvokeEndMonthEvents()
    {
        if (_EndMonthEvents != null)
            _EndMonthEvents();
    }

    public void InvokeNewMonthEvents()
    {
        if (_NewMonthEvents != null)
            _NewMonthEvents();
    }

    public void InvokeEndYearEvents()
    {
        if (_EndYearEvents != null)
            _EndYearEvents();
    }

    public void InvokeNewYearEvents()
    {
        if (_NewYearEvents != null)
            _NewYearEvents();
    }
    #endregion

    #region Budget
    public void SpendingComplete(SpendsSubjects Subject, int UnitID, int Authority)
    {
        World.World wrld = World.World.TheWorld;
        switch (Subject)
        {
            case SpendsSubjects.MilitaryUnit:
                wrld.GetMainMilPool(Authority).AddUnits(UnitID, 1);
                break;
            case SpendsSubjects.CosmoUnit:
                break;
            case SpendsSubjects.Upgrade:
                break;
            case SpendsSubjects.TechMilitary:
                break;
            case SpendsSubjects.TechProduction:
                break;
            default:
                break;
        }
    }
    #endregion
}
