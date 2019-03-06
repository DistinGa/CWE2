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

    public sealed class EndOfCombat_EventArgs : EventArgs
    {
        public int CombatID;
        public int WinnerRegID; // Победивший в бою регион
    }
}
