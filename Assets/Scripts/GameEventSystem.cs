using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void SimpleEvent();

public sealed class GameEventSystem
{
    private static GameEventSystem _Instance;

    private SimpleEvent _TurnEvents, _MonthEvents, _EndYearEvents, _NewYearEvents;

    public static GameEventSystem Instance
    {
        get
        {
            if(_Instance == null)
                _Instance = new GameEventSystem();

            return _Instance;
        }
    }

    #region Subscribes
    public void SubscribeOnTurn(SimpleEvent dlg, bool Subscribe = true)
    {
        if(Subscribe)
            _TurnEvents += dlg;
        else
            _TurnEvents -= dlg;
    }

    public void SubscribeOnMonth(SimpleEvent dlg, bool Subscribe = true)
    {
        if (Subscribe)
            _MonthEvents += dlg;
        else
            _MonthEvents -= dlg;
    }

    public void SubscribeOnEndYear(SimpleEvent dlg, bool Subscribe = true)
    {
        if (Subscribe)
            _EndYearEvents += dlg;
        else
            _EndYearEvents -= dlg;
    }

    public void SubscribeOnNewYear(SimpleEvent dlg, bool Subscribe = true)
    {
        if (Subscribe)
            _NewYearEvents += dlg;
        else
            _NewYearEvents -= dlg;
    }
    #endregion

    #region Invokes
    public void InvokeTurnEvents()
    {
        _TurnEvents();
    }

    public void InvokeMonthEvents()
    {
        _MonthEvents();
    }

    public void InvokeEndYearEvents()
    {
        _EndYearEvents();
    }

    public void InvokeNewYearEvents()
    {
        _NewYearEvents();
    }
    #endregion


}
