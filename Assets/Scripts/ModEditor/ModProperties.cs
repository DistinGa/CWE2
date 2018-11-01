using System.Collections;
using System.Collections.Generic;

namespace ModEditor
{
    public class ModProperties
    {
        public static ModProperties Instance;

        public const float YEAR_TURNS_COUNT = 48; //Количество ходов в году (float, потому что на это число чаще всего будут делить целые числа)
        public int GameYearsCount;  //Количество лет игры, после которого игра заканчивается

        public List<string> Authorities;  //Список режимов. Нулевой считается нейтральным

        //Ежегодный прирост GNP в неконтролируемых странах.
        public int GNP_Neutral_Min, GNP_Neutral_Max, GNP_HighDevLevel_Min, GNP_HighDevLevel_Max, GNP_LowDevLevel_Min, GNP_LowDevLevel_Max;  //Интервалы изменения GNP нейтральных стран
        public int ProspMaxValue;    //+/- для Radicalizm и Prosperity параметра DevLevel
        public int DefaultMilBaseCapacity;  //Вместимость новой базы
        public float UnitMovementSpeed;     //Скорость перемещения военных юнитов между пулами
        public int UnitMovementCost;        //Стоимость перемещения военных юнитов между пулами ($ за ход)
        public double ProsperityAdditionToNatFund;  //Сколько процентов добавлять в нацфонд за один уровень благосостояния (0 - 100)
        public float PrivateWeeklyGrow;    //Ежемесячный рост частного сектора (0-1%)
        public float PrivateLoadedWeeklyGrow;    //Ежемесячный рост частного сектора (0-1%)
        public float PrivateFactor;         //Во сколько раз дороже постройка юнитов за счёт частного сектора
        public float MilitarySystemCostIncreasePerUpgrade;  //Увеличение стоимости производства военной системы при апгрейде
        public float MilitarySystemCostDecreaseByUpgrade;   //Снижение стоимости производства военной системы от апгрейда типа 4
        public int MilitarySystemCapacityUpgrade;   //Увеличение вместимости (body) или уменьшение занимаемого места других систем при апгрейде
        public int MilitarySystemParamIncreaseByUpgrade;    //На сколько увеличивается один из двух параметров системы при апгрейде.
        public List<string> MilitaryUnitClasses;   //Helicopter / Tank / Submarine ...
        public List<string> MilitaryUnitTypes;     //Land / Sea / Air

        public List<nsEmbassy.DiplomaticMission> DipMissionsList;   //Список всех существующих в игре дип. миссий
        public List<string> EmbassyLevelsNames;     //Наименования уровней посольства
        public List<int> EmbassyLevelSizes;      //Количество шпионских сетей по уровням посольств
        public float InitEmbassyUpgradeCost;         //Стоимость первого апгрейда посольства
        public float InitSpyNetUpgradeCost;         //Стоимость первого апгрейда шпионской сети
        public int InitSpyNetSuccess;             //Начальный процент успешности сети
        public float EmbassyUpgradeCostFactor;       //Во сколько раз увеличивается стоимость следующего апгрейда
        public float SpyNetSuccessUpgradePercent;   //На сколько процентов увеличивается успех миссии сети
        public int SpyNetSpeedUpgradePercent;     //На сколько процентов увеличивается скорость выполнения миссии сети
        public int SpyNetCounterEspionageDelayTime; //Время, на которое блокируется вражеская сеть при контр-шпионаже

        public List<PoliticParty_Props> PoliticParties;  //Список разновидностей политических партий (во всех регионах такой набор партий)
        //public List<PoliticLaw_Props> PoliticLaws;        //Список всех аозможных в игре законов
        public float AnnualPartyPopularityGain;    //Ежегодный прирост популярности партии (в неконтролируемом регионе меняется раз в год, в контролируемом - каждый ход на кратное (1/48) значение)
        public float RelativeNatFundToPartyGrow;    //Долг в нац фонде, который считается за 100% при расчёте скорости роста партии, зависящей от него
        public int PassingLawTime1;                 //Время принятия закона лидирующей партией (ходы)
        public int PassingLawTime2;                 //Время принятия закона второй лидирующей партией (ходы)
        public int PassingLawTime_;                 //Время принятия закона остальными партиями (ходы)

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
