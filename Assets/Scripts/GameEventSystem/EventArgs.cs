using System;

namespace nsEventSystem
{
    // Предполагается передача данных для событий с клиента на сервер, поэтому аргументы должны сериализоваться.
    // Их состав следует проектировать исходя из этого.

    public sealed class ProduceNewUnits_EventArgs : EventArgs
    {
        public int RegID, UnitID, Amount;
    }

    public sealed class AbortPartyLawInRegion_EventArgs : EventArgs
    {
        public int RegID, PartyID;
    }

    public sealed class AttackBattleAction_EventArgs : EventArgs
    {
        public string Message;
    }

    public sealed class EndOfCombat_EventArgs : EventArgs
    {
        public int CombatID;
        public int WinnerRegID; // Победивший в бою регион
    }

    public sealed class AddUnitsToWar_EventArgs : EventArgs
    {
        public int WarID;
        public bool ForAttacker; // Войска для агрессора
        public int MilUnitID;
        public int BaseID;   // Откуда войска (-1 - домашний пул; -11 - морской пул; >= 0 - военная база);
        public int Amount;
    }

    public sealed class AddNews_EventArgs : EventArgs
    {
        public int RegionID;    // Место действия
        public int InitTurn;    // Ход, в который произошло событие
        public string TextID;   // Ключ текста локализации
    }

    public sealed class ChangeAuthority_EventArgs : EventArgs
    {
        public int RegionID;
        public int NewAuthorityID;
    }

    public sealed class AddIntPropertyInRegion_EventArgs : EventArgs
    {
        public int RegionID;
        public int Amount;
    }

    public sealed class Spends_EventArgs : EventArgs
    {
        public string BudgetItem;
        public nsWorld.Spends SpendsRef;
    }

    public sealed class TwoInt_EventArgs : EventArgs
    {
        public int int1, int2;
    }

    public sealed class ThreeInt_EventArgs : EventArgs
    {
        public int int1, int2, int3;
    }
}
