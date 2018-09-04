using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ModEditor
{
    public class ModProperties
    {
        public static ModProperties Instance;

        //Ежегодный прирост GNP в неконтролируемых странах.
        public int GNP_Neutral_Min, GNP_Neutral_Max, GNP_HighDevLevel_Min, GNP_HighDevLevel_Max, GNP_LowDevLevel_Min, GNP_LowDevLevel_Max;  //Интервалы изменения GNP нейтральных стран
        public int ProspMaxValue;    //+/- для Radicalizm и Prosperity параметра DevLevel
        public int DefaultMilBaseCapacity;  //Вместимость новой базы
        public float UnitMovementSpeed;     //Скорость перемещения военных юнитов между пулами
        public int UnitMovementCost;        //Стоимость перемещения военных юнитов между пулами ($ за ход)
        public double ProsperityAdditionToNatFund;  //Сколько процентов добавлять в нацфонд за один уровень благосостояния
        public float PrivateWeeklyGrow;    //Ежемесячный рост частного сектора (0-1%)
        public float PrivateLoadedWeeklyGrow;    //Ежемесячный рост частного сектора (0-1%)
        public float PrivateFactor;         //Во сколько раз дороже постройка юнитов за счёт частного сектора
        public float MilitarySystemCostIncreasePerUpgrade;  //Увеличение стоимости производства военной системы при апгрейде
        public float MilitarySystemCostDecreaseByUpgrade;   //Снижение стоимости производства военной системы от апгрейда типа 4
        public int MilitarySystemCapacityUpgrade;   //Увеличение вместимости (body) или уменьшение занимаемого места других систем при апгрейде
        public int MilitarySystemParamIncreaseByUpgrade;    //На сколько увеличивается один из двух параметров системы при апгрейде.

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
