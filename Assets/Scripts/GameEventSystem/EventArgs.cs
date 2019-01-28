using System;

namespace nsEventSystem
{
    public sealed class ProduceNewUnits_EventArgs : EventArgs
    {
        public int AuthID, UnitID, Amount;
    }

    public sealed class AbortPartyLawInRegion_EventArgs : EventArgs
    {
        public int RegID, PartyID;
    }

    public sealed class EndOfCombat_EventArgs : EventArgs
    {
        public int WinnerRegID;
    }
}
