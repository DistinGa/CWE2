using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using nsWorld;
using nsEventSystem;

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
    Upgrade
}

public class RegionController
{
    public int AuthorityID { get; private set; }
    public int HomelandID { get; private set; }    //ID контролируемого региона

    RegionController_Ds _RegCData;
    bool f_TurnIsDone;    //флаг, показывающий, что ход сделан
    nsAI.AI AI;

    public RegionController(int Authority, int HomelandID, nsAI.AI AI = null)
    {
        AuthorityID = Authority;
        //this.Color = Color;
        this.HomelandID = HomelandID;
        this.AI = AI;

        _RegCData = new RegionController_Ds();

        GameEventSystem.Subscribe(GameEventSystem.MyEventsTypes.TurnActions, OnTurn);
        GameEventSystem.Subscribe(GameEventSystem.MyEventsTypes.EndMonthEvents, EndOfMonth);
    }

    ~RegionController()
    {
        GameEventSystem.UnSubscribe(GameEventSystem.MyEventsTypes.TurnActions, OnTurn);
        GameEventSystem.UnSubscribe(GameEventSystem.MyEventsTypes.EndMonthEvents, EndOfMonth);
    }

    #region Properties

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

    public int ProsperityLevel
    {
        get { return World.TheWorld.GetRegion(HomelandID).ProsperityLevel; }

        set { World.TheWorld.GetRegion(HomelandID).ProsperityLevel = value; }
    }

    public int Inflation
    {
        get { return _RegCData.Inflation; }

        set
        {
            _RegCData.Inflation = value;
            if (_RegCData.Inflation < 0)
                _RegCData.Inflation = 0;
            if (_RegCData.Inflation > 100)
                _RegCData.Inflation = 100;
        }
    }

    public int Corruption
    {
        get { return _RegCData.Corruption; }

        set
        {
            _RegCData.Corruption = value;
            if (_RegCData.Corruption < 0)
                _RegCData.Corruption = 0;
            if (_RegCData.Corruption > 100)
                _RegCData.Corruption = 100;
        }
    }

    public int TechMilitary
    {
        get { return _RegCData.TechMilitary; }

        set
        {
            _RegCData.TechMilitary = value;
            if (_RegCData.TechMilitary < 0)
                _RegCData.TechMilitary = 0;
        }
    }

    public int TechProduction
    {
        get { return _RegCData.TechProduction; }

        set
        {
            _RegCData.TechProduction = value;
            if (_RegCData.TechProduction < 0)
                _RegCData.TechProduction = 0;
        }
    }

    public Region_Op ControlledRegion
    {
        get { return World.TheWorld.GetRegion(HomelandID); }
    }

    public bool TurnIsDone
    {
        get { return f_TurnIsDone; }
    }

    #endregion

    private void OnTurn(object sender, EventArgs e)
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
        BudgetItem bi = _RegCData.BudgetItems[BudgetItem.BI_PrivateSector];
        bi.Value *= 1f + 0.01f * bi.WeeklyGrow;
        //Рост частного сектора от использования
        bi.Value += GetSpends(SpendsSource.Private) * (1f + 0.01f * bi.LoadedWeeklyGrow);

        //Изменение популярности партий каждый ход
        for (int i = 0; i < ModEditor.ModProperties.Instance.PoliticParties.Count; i++)
        {
            float x = ModEditor.ModProperties.Instance.PoliticParties[i].GetPartyPopularityGain(this);
            ControlledRegion.AddPartyPopularity(i, x);
        }
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

        double economy = _RegCData.BudgetItems.Values.Where(bi => bi.Ministry == BudgetMinistry.Economy).Sum(bi => bi.Value);
        double social = _RegCData.BudgetItems.Values.Where(bi => bi.Ministry == BudgetMinistry.Social).Sum(bi => bi.Value);
        double privateSpends = spends[1], nationalizeSpends = spends[0];

        _RegCData.NatFund += (economy - privateSpends - nationalizeSpends - social)
            * 0.01d * (100d + ProsperityLevel * ModEditor.ModProperties.Instance.ProsperityAdditionToNatFund - _RegCData.Corruption - _RegCData.Inflation);
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

    /// <summary>
    /// Начало хода игрока (человек или ИИ)
    /// </summary>
    public void TurnStart()
    {
        f_TurnIsDone = false;

        if (AI != null)
        {
            //Действия ИИ
            AI.Turn(this.HomelandID);
            TurnComplete();
        }
        else
        {
            //Если человек (Нужно активировать кнопку "Ход")


        }
    }

    /// <summary>
    /// Вызывается извне для сообщения о совершении хода
    /// </summary>
    public void TurnComplete()
    {
        f_TurnIsDone = true;
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

            GameEventSystem.SpendingComplete(_Subject, _ID, _Authority);
 
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
    public double NatFund;
    public Dictionary<string, BudgetItem> BudgetItems;
    public int Corruption;   //0 - 100
    public int Inflation;    //0 - 100
    public int TechMilitary;    //MilitaryGeneration
    public int TechProduction;
}
