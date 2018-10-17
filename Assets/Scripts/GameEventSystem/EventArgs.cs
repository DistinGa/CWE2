using System;

namespace nsEventSystem
{
    public sealed class ProduceNewUnits_EventArgs : EventArgs
    {
        public int AuthID, UnitID, Amount;
    }

    public sealed class SpyNetCompletesDipMission_EventArgs : EventArgs
    {
        public nsEmbassy.SpyNet SpyNet;
    }
}
