using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using nsWorld;
using nsEventSystem;

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
        this.HomelandID = HomelandID;
        this.AI = AI;

        _RegCData = new RegionController_Ds();

        GameEventSystem.Subscribe(GameEventSystem.MyEventsTypes.TurnActions, OnTurn);
        GameEventSystem.Subscribe(GameEventSystem.MyEventsTypes.EndMonthEvents, EndOfMonth);
        GameEventSystem.Subscribe(GameEventSystem.MyEventsTypes.EndYearEvents, EndOfYear);
        GameEventSystem.Subscribe(GameEventSystem.MyEventsTypes.UpgradeEmbassyOuter, UpgradeEmbassy);
        GameEventSystem.Subscribe(GameEventSystem.MyEventsTypes.UpgradeMilBaseOuter, UpgradeMilBase);
    }

    ~RegionController()
    {
        GameEventSystem.UnSubscribe(GameEventSystem.MyEventsTypes.TurnActions, OnTurn);
        GameEventSystem.UnSubscribe(GameEventSystem.MyEventsTypes.EndMonthEvents, EndOfMonth);
        GameEventSystem.UnSubscribe(GameEventSystem.MyEventsTypes.EndYearEvents, EndOfYear);
        GameEventSystem.UnSubscribe(GameEventSystem.MyEventsTypes.UpgradeEmbassyOuter, UpgradeEmbassy);
        GameEventSystem.UnSubscribe(GameEventSystem.MyEventsTypes.UpgradeMilBaseOuter, UpgradeMilBase);
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

    public double Collaboration
    {
        get
        {
            double res = 0d;

            foreach (Region_Op item in nsWorld.World.TheWorld.Regions.Values.Where(r => r.RegionController == this).ToList())
            {
                res += item.GNP;
            }

            return res;
        }
    }

    public List<double> NatFundHistory
    {
        get { return _RegCData.NatFundHistory; }
    }

    public List<double> CollaborationHistory
    {
        get { return _RegCData.CollaborationHistory; }
    }

    public List<int> InflationHistory
    {
        get { return _RegCData.InflationHistory; }
    }

    public List<int> CorruptionHistory
    {
        get { return _RegCData.CorruptionHistory; }
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

    public int DevLevel
    {
        get { return _RegCData.DevLevel; }
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
        foreach (var item in _RegCData.BudgetItems)
        {
            item.Value.DistributeFundsToSpends();
        }
        //for (int i = _RegCData.Spends.Count - 1; i >= 0; i--)
        //{
        //    Spends item = _RegCData.Spends[i];
        //    if (item.Amount == 0d && item.Accumulation == 0d)
        //    {
        //        _RegCData.Spends.Remove(item);
        //        continue;
        //    }

        //    item.DoSpend();
        //}

        //Рост частного сектора
        BudgetItem bi = _RegCData.BudgetItems[BudgetItem.BI_PrivateSector];
        bi.Value *= 1f + 0.01f * bi.WeeklyGrow;
        //Рост частного сектора от использования
        bi.Value += GetSpends(BudgetItem.BI_PrivateSector) * (1f + 0.01f * bi.LoadedWeeklyGrow);

        //Изменение популярности партий каждый ход
        for (int i = 0; i < GameManager.GM.GameProperties.PoliticParties.Count; i++)
        {
            float x = GameManager.GM.GameProperties.PoliticParties[i].GetPartyPopularityGain(this);
            ControlledRegion.AddPartyPopularity(i, x);
        }
    }

    private void EndOfMonth(object sender, EventArgs e)
    {
        //Обновление бюджета
        MonthBudgetChahge();
    }

    private void EndOfYear(object sender, EventArgs e)
    {
        //История разделов бюджета
        _RegCData.NatFundHistory.Add(_RegCData.NatFund);
        _RegCData.CollaborationHistory.Add(Collaboration);
        _RegCData.CorruptionHistory.Add(_RegCData.Corruption);
        _RegCData.InflationHistory.Add(_RegCData.Inflation);
    }

    private void NewYear(object sender, EventArgs e)
    {

    }

    //Траты определённого вида
    public double GetSpends(string SpType)
    {
        double res = 0;
        
        foreach (var item in _RegCData.BudgetItems[SpType].Spends)
        {
            res += item.TurnSpends;
        }

        return res;
    }

    //Ежемесячный прирост бюджета
    private void MonthBudgetChahge()
    {
        double economy = _RegCData.BudgetItems.Values.Where(bi => bi.Ministry == BudgetMinistry.Economy).Sum(bi => bi.Value);
        double social = _RegCData.BudgetItems.Values.Where(bi => bi.Ministry == BudgetMinistry.Social).Sum(bi => bi.Value);
        double privateSpends = GetSpends(BudgetItem.BI_PrivateSector), nationalizeSpends = GetSpends(BudgetItem.BI_NatEconomy);

        _RegCData.NatFund += (economy - privateSpends - nationalizeSpends - social - Collaboration)
            * 0.01d * (100d + ProsperityLevel * GameManager.GM.GameProperties.ProsperityAdditionToNatFund - _RegCData.Corruption - _RegCData.Inflation);
    }

    /// <summary>
    /// Апгрейд посольства
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e">int1 - RegID, int2 - Authority, int3 - 0/1</param>
    private void UpgradeEmbassy(object sender, EventArgs e)
    {
        ThreeInt_EventArgs _args = e as ThreeInt_EventArgs;

        if (_args.int2 != AuthorityID)
            return; //Событие не для этого контроллера

        nsEmbassy.Embassy _embassy = World.TheWorld.Embassies[_args.int1][_args.int2];

        if (_embassy.HighestEmbassy)
            return; //Посольство и так на последнем уровне

        if (PayCount(_args.int3, _embassy.EmbassyUpgradeCost))
            return; //Не хватает престижа или денег.

        _embassy.Upgrade();
    }

    /// <summary>
    /// Апгрейд военной базы
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e">int1 - RegID, int2 - Authority, int3 - 0/1</param>
    private void UpgradeMilBase(object sender, EventArgs e)
    {
        ThreeInt_EventArgs _args = e as ThreeInt_EventArgs;

        if (_args.int2 != AuthorityID)
            return; //Событие не для этого контроллера

        if (PayCount(_args.int3, nsMilitary.MilitaryManager.Instance.GetMilitaryBaseUpgradeCost(World.TheWorld.Regions[_args.int1].MilitaryBaseID)))
            return; //Не хватает престижа или денег.

        nsMilitary.MilitaryManager.Instance.UpgradeMilitaryBase(_args.int1, AuthorityID);
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

    /// <summary>
    /// Элемент бюджета с указанным индексомм
    /// </summary>
    /// <param name="BudgetItemIndex"></param>
    /// <returns></returns>
    public BudgetItem GetBudgetItem(string BudgetItemIndex)
    {
        return _RegCData.BudgetItems[BudgetItemIndex];
    }

}

public class RegionController_Ds
{
    public int PP;  //Political points
    public int Prestige;

    //Бюджет
    public Dictionary<string, BudgetItem> BudgetItems;
    public double NatFund;
    public int Corruption;   //0 - 100
    public int Inflation;    //0 - 100
    public List<double> NatFundHistory;
    public List<double> CollaborationHistory;
    public List<int> CorruptionHistory;
    public List<int> InflationHistory;
    public int TechMilitary;    //MilitaryGeneration
    public int TechProduction;
    public int DevLevel;
}
