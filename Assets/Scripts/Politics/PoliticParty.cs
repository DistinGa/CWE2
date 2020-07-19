using System.Collections;
using System.Collections.Generic;
using ModEditor;

namespace nsWorld
{
    public class PoliticParty: ISavable
    {
        public int ppID;
        public float Popularity;
        public List<int> PoliticalLawIDs;   //Очередь законов для принятия
        public float RemainTurns;  //оставшееся время до принятия

        public PoliticParty(int ppID, float Popularity)
        {
            this.ppID = ppID;
            this.Popularity = Popularity;
        }

        /// <summary>
        /// Ссылка на параметры партии из настроек
        /// </summary>
        public PoliticParty_Props Party_Prop
        {
            get { return GameManager.GM.GameProperties.PoliticParties[ppID]; }
        }
    }
}
