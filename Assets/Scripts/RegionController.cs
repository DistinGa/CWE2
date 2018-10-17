using System;
using System.Collections;
using System.Collections.Generic;
using nsWorld;
using nsEventSystem;
using UnityEngine;

public enum SpendsSource
{
    Nationalize,
    Private,
    Science
}

public enum SpendsSubjects
{
    MilitaryUnit,
    CosmoUnit,
    Upgrade,
    TechMilitary,
    TechProduction
}

public enum SpendsGoals
{
    MilitaryUnit,
    CosmoUnit,
    Technology,
    Ungrade
}

public class RegionController
{
    RegionController_Ds _RegCData;
    int _Authority;
    Color _Color;
    int _HomelandID;

    public RegionController(RegionController_Ds RegCData, int Authority, Color Color, int HomelandID)
    {
        _RegCData = RegCData;
        _Authority = Authority;
        _Color = Color;
        _HomelandID = HomelandID;

        GameEventSystem.Instance.Subscribe(GameEventSystem.MyEventsTypes.TurnEvents, Turn);
        GameEventSystem.Instance.Subscribe(GameEventSystem.MyEventsTypes.EndMonthEvents, EndOfMonth);
    }

    ~RegionController()
    {
        GameEventSystem.Instance.UnSubscribe(GameEventSystem.MyEventsTypes.TurnEvents, Turn);
        GameEventSystem.Instance.UnSubscribe(GameEventSystem.MyEventsTypes.EndMonthEvents, EndOfMonth);
    }

    public int Prestige
    {
        get { return _RegCData.Prestige; }

        set
        {
            _RegCData.Prestige = value;
            if (_RegCData.Prestige < 0)
                throw new Exception("Prestige < 0");
        }
    }

    public double NatFund
    {
        get { return _RegCData.NatFund; }

        set
        {
            _RegCData.NatFund = value;
        }
    }

    private void Turn(object sender, EventArgs e)
    {
        //Производство юнитов, апгрейд и изучение технологий
        for (int i = _RegCData.Spends.Count - 1; i >= 0; i--)
        {
            Spends item = _RegCData.Spends[i];
            if (item.Amount == 0d && item.Accumulation == 0d)
            {
                _RegCData.Spends.Remove(item);
                continue;
            }

            item.DoSpend();
        }

        //Рост частного сектора
        _RegCData.PrivateEconomy *= 1f + 0.01f * ModEditor.ModProperties.Instance.PrivateWeeklyGrow;
        //Рост частного сектора от использования
        _RegCData.PrivateEconomy += GetSpends(SpendsSource.Private) * (1f + 0.01f * ModEditor.ModProperties.Instance.PrivateLoadedWeeklyGrow);
    }

    private void EndOfMonth(object sender, EventArgs e)
    {
        //Обновление бюджета
        MonthBudgetChahge();
    }

    private void EndOfYear()
    {

    }

    private void NewYear()
    {

    }

    //Траты определённого вида
    public double GetSpends(SpendsSource SpType)
    {
        double res = 0;

        foreach (var item in _RegCData.Spends)
        {
            if (item.SpendsType == SpType)
                res += item.Amount;
        }

        return res;
    }

    //Траты всех видов в массиве ((0) - Nationalize, (1) - Private, (2) - Science)
    public double[] GetSpends()
    {
        double[] res = new double[3];

        foreach (var item in _RegCData.Spends)
        {
            switch (item.SpendsType)
            {
                case SpendsSource.Nationalize:
                    res[0] += item.Amount;
                    break;
                case SpendsSource.Private:
                    res[1] += item.Amount;
                    break;
                case SpendsSource.Science:
                    res[2] += item.Amount;
                    break;
                default:
                    break;
            }
        }

        return res;
    }

    //Ежемесячный прирост бюджета
    private void MonthBudgetChahge()
    {
        double[] spends = GetSpends();

        _RegCData.NatFund += ((_RegCData.NationalizeEconomy - spends[0] + _RegCData.PrivateEconomy - spends[1]) - (_RegCData.Social + spends[2]))
            * 0.01d * (100d + _RegCData.ProsperityLevel * ModEditor.ModProperties.Instance.ProsperityAdditionToNatFund - _RegCData.Corruption - _RegCData.Inflation);
    }

    /// <summary>
    /// Оплата чего-либо. Может производиться за счёт Престижа или из Нацфонда.
    /// </summary>
    /// <param name="SourceID">0 - из нацфонда; 1 - из престижа</param>
    /// <param name="Cost">Стоимость</param>
    /// <returns>true, если оплата прошла успешно</returns>
    public bool PayCount(int SourceID, double Cost)
    {
        if (SourceID == 1 && Prestige < (int)Cost)
            return false; //Не хватает престижа.

        if (SourceID == 0)
            NatFund -= Cost;   //Может уходить в минус
        else
            Prestige -= (int)Cost;

        return true;
    }



    public class Spends
    {
        int _Authority;
        SpendsSubjects _Subject;
        SpendsSource _Type;
        SpendsGoals _Goal;
        double _Amount; //траты за ход
        double _Accumulation;   //уже накоплено
        int _ID; //ID юнита, технологии или апгрейда

        public Spends(int Authority, SpendsSubjects Subject, SpendsSource Type, SpendsGoals Goal, double Amount, int ObjID)
        {
            _Authority = Authority;
            _Subject = Subject;
            _Type = Type;
            _Goal = Goal;
            _Amount = Amount;
            _ID = ObjID;
        }

        public SpendsSource SpendsType
        {
            set
            {
                //Если меняем источник инвестирования на Private, накопленную сумму увеличиваем. В итоге процент завершённости юнита остаётся прежним, изменяется стоимость "остаточной постройки"
                if (_Type == SpendsSource.Nationalize && value == SpendsSource.Private)
                    _Accumulation *= ModEditor.ModProperties.Instance.PrivateFactor;
                //Аналогично поступаем в обратную сторону.
                if(_Type == SpendsSource.Private && value == SpendsSource.Nationalize)
                    _Accumulation /= ModEditor.ModProperties.Instance.PrivateFactor;

                _Type = value;
            }
            get { return _Type; }
        }

        //Определение стоимости юнита, технологии или апгрейда
        public double Cost
        {
            get
            {
                double res = 0;

                switch (_Type)
                {
                    case SpendsSource.Nationalize:
                        if (_Goal == SpendsGoals.MilitaryUnit)
                            res = nsMilitary.MilitaryManager.Instance.GetMilitaryUnit(_ID).Cost;
                        else if (_Goal == SpendsGoals.CosmoUnit)
                            res = 0;
                        break;
                    case SpendsSource.Private:
                        if (_Goal == SpendsGoals.MilitaryUnit)
                            res = nsMilitary.MilitaryManager.Instance.GetMilitaryUnit(_ID).Cost * ModEditor.ModProperties.Instance.PrivateFactor;
                        else if (_Goal == SpendsGoals.CosmoUnit)
                            res = 0;
                        break;
                    case SpendsSource.Science:
                        
                        break;
                    default:
                        break;
                }

                return res;
            }
        }

        public double Accumulation
        {
            get { return _Accumulation; }
        }

        public double Amount
        {
            set { _Amount = value; }
            get { return _Amount; }
        }

        public void DoSpend()
        {
            _Accumulation += _Amount;
            Execute();
        }

        void Execute()
        {
            if (_Accumulation < Cost)
                return;

            GameEventSystem.Instance.SpendingComplete(_Subject, _ID, _Authority);
 
            _Accumulation -= Cost;
            Execute();
       }
    }
}

public class RegionController_Ds
{
    public int PP;  //Political points
    public int Prestige;
    public List<RegionController.Spends> Spends;

    //Бюджет
    public double PrivateEconomy, NationalizeEconomy, Social, NatFund;
    public double Corruption;   //0 - 100
    public double Inflation;    //0 - 100
    public int ProsperityLevel; //+-ProspMaxValue

}
