using System.Collections;
using System.Collections.Generic;
using World;
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

        GameEventSystem.Instance.SubscribeOnEndMonth(Turn);
        GameEventSystem.Instance.SubscribeOnEndMonth(EndOfMonth);
    }

    ~RegionController()
    {
        GameEventSystem.Instance.SubscribeOnEndMonth(Turn, false);
        GameEventSystem.Instance.SubscribeOnEndMonth(EndOfMonth, false);
    }

    private void Turn()
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

    private void EndOfMonth()
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
                            res = World.World.TheWorld.GetMilitaryUnit(_ID).Cost;
                        else if (_Goal == SpendsGoals.CosmoUnit)
                            res = 0;
                        break;
                    case SpendsSource.Private:
                        if (_Goal == SpendsGoals.MilitaryUnit)
                            res = World.World.TheWorld.GetMilitaryUnit(_ID).Cost * ModEditor.ModProperties.Instance.PrivateFactor;
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
    public double Corruption;
    public double Inflation;
    public int ProsperityLevel;

}
