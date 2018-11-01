using System.Collections;
using System.Collections.Generic;
using ModEditor;

namespace nsWorld
{
    public class PoliticParty
    {
        public int ppID;
        public float Popularity;
        public List<int> PoliticalLawIDs;
        public float RemainTurns;  //оставшееся время до принятия

        /// <summary>
        /// Ссылка на параметры партии из настроек
        /// </summary>
        public PoliticParty_Props Party_Prop
        {
            get { return ModProperties.Instance.PoliticParties[ppID]; }
        }
    }
}
