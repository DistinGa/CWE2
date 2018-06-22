using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ModEditor
{
    public class ModProperties
    {
        public static ModProperties Instance;

        //Ежегодный прирост GNP в неконтролируемых странах.
        public int GNP_Neutral_Min, GNP_Neutral_Max, GNP_HighDevLevel_Min, GNP_HighDevLevel_Max, GNP_LowDevLevel_Min, GNP_LowDevLevel_Max;
        public int RadProspMaxValue;    //+/- для Radicalizm и Prosperity параметра DevLevel
        public int DefaultMilBaseCapacity;  //Вместимость новой базы

        private ModProperties()
        {
            Instance = this;
        }

        public void CreateModProperties()
        {
            if (Instance == null)
                new ModProperties();
        }
    }
}
