using System.Collections;
using System.Collections.Generic;
using Assets.SimpleLocalization;

namespace nsNews
{
    public class News
    {
        public int RegionID { get; }    // Место действия
        public int InitTurn { get; }
        public string TextID { get; }

        public News(int regionID, int initTurn, string textID)
        {
            RegionID = regionID;
            InitTurn = initTurn;
            TextID = textID;
        }

        public string GetNewsText()
        {
            return LocalizationManager.Localize(TextID);
        }
    }
}
