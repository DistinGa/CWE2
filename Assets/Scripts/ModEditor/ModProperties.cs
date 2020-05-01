using System.Collections;
using System.Collections.Generic;

namespace ModEditor
{
    [System.Serializable]
    public class ModProperties
    {
        public static ModProperties Instance;

        public int TickInterval;    //Время одного хода в секундах.
        public const float YEAR_TURNS_COUNT = 48; //Количество ходов в году (float, потому что на это число чаще всего будут делить целые числа)
        public int GameYearsCount;  //Количество лет игры, после которого игра заканчивается

        public List<string> Authorities;  //Список политических режимов (-1 - нейтральный).
        public List<int> ControlledRegions;  //Список контролируемых регионов. Индекс соответствует Authority.
        public List<Region_Prop> Regions;        //Список неизменяемых свойств регионов
        public List<nsWorld.Region_Ds> Regions_Originals;        //Список изменяемых свойств регионов (Индекс совпадает с Regions)

        //Ежегодный прирост GNP в неконтролируемых странах.
        public int GNP_Neutral_Min, GNP_Neutral_Max, GNP_HighDevLevel_Min, GNP_HighDevLevel_Max, GNP_LowDevLevel_Min, GNP_LowDevLevel_Max;  //Интервалы изменения GNP нейтральных стран
        public int ProspMaxValue;    //+/- для Radicalizm и Prosperity параметра DevLevel
        public int DefaultMilBaseCapacity;  //Вместимость новой базы
        public float UnitMovementSpeed;     //Скорость перемещения военных юнитов между пулами
        public int UnitMovementCost;        //Стоимость перемещения военных юнитов между пулами ($ за ход)
        public double ProsperityAdditionToNatFund;  //Сколько процентов добавлять в нацфонд за один уровень благосостояния (0 - 100)
        public float PrivateWeeklyGrow;    //Еженедельный рост частного сектора (0-1%)
        public float PrivateLoadedWeeklyGrow;    //Еженедельный рост загруженного частного сектора (0-1%)
        //public float PrivateFactor;         //Во сколько раз дороже постройка юнитов за счёт частного сектора
        public float MilitarySystemCostIncreasePerUpgrade;  //Увеличение стоимости производства военной системы при апгрейде
        public float MilitarySystemCostDecreaseByUpgrade;   //Снижение стоимости производства военной системы от апгрейда типа 4
        public int MilitarySystemCapacityUpgrade;   //Увеличение вместимости (body) или уменьшение занимаемого места других систем при апгрейде
        public int MilitarySystemParamIncreaseByUpgrade;    //На сколько увеличивается один из двух параметров системы при апгрейде.
        public List<string> MilitaryUnitClasses;   //Helicopter / Tank / Submarine ...
        public List<string> MilitaryUnitTypes;     //Land / Sea / Air

        //Посольства
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

        //Парламент
        public List<PoliticParty_Props> PoliticParties = new List<PoliticParty_Props>();  //Список разновидностей политических партий (во всех регионах такой набор партий)
        //public List<PoliticLaw_Props> PoliticLaws;        //Список всех возможных в игре законов
        public float AnnualPartyPopularityGain;    //Ежегодный прирост популярности партии (в неконтролируемом регионе меняется раз в год, в контролируемом - каждый ход на кратное (1/48) значение) (1 - 100)
        public float AnnualRadicalsPopularityGain;    //Ежегодный прирост популярности групп радикалов (1 - 100)
        public float RelativeNatFundToPartyGrow;    //Долг в нац фонде, который считается за 100% при расчёте скорости роста партии, зависящей от него
        public int PassingLawTime1;                 //Время принятия закона лидирующей партией (ходы)
        public int PassingLawTime2;                 //Время принятия закона второй лидирующей партией (ходы)
        public int PassingLawTime_;                 //Время принятия закона остальными партиями (ходы)

        //Military
        public List<nsMilitary.UnitClass> UnitClasses;  //Классы юнитов (танк, истребитель, пехота и тд.)
        public double GlobalDevLimit;   // Лимит выделенных средств на производство одного юнита (изучение одной системы). (В деньгах)
        public float InitialDevLimit;   // Общий лимит выделения средств из бюджета на производство/изучение в начале игры

        //Combat
        public LocalizedDictionary<nsCombat.WarPhase> WarPhasesNames;   // Названия фаз войны
        public Dictionary<int, nsCombat.ReliefProperties> ReliefProperties; // Список всех особенностей рельефа
        public Dictionary<int, Dictionary<nsCombat.WarPhase, int>> RegPhaseReliefProperties;  //Особенности рельефа для региона и фазы войны (<RegionID, <WarPhase, ReliefPropertiesID>>)
        public int FireCost;    //Сколько Supply отнимается при выстреле
        public int CombatArea, CenterCombatArea;    //Размер поля боя, и центральная облать (количество линий для каждой стороны)
        public int AggressorMoralPenalty;           //Штраф к морали за агрессию
        public int RetreatMoralPenalty;           //Штраф к морали за проигрыш в фазе войны
        public int NonePhaseTurns;              // Количество ходов без военных действий, после которого начинается контратака государства.

        private ModProperties()
        {
            Instance = this;
        }

        /// <summary>
        /// Инициализация экземпляра.
        /// </summary>
        /// <param name="FileName">Имя файла, из которого загружаются настройки. Если опущено, загружаются дефолтные, заданные при создании мода.</param>
        public static void CreateModProperties(string FileName = "")
        {
            if (Instance == null)
                new ModProperties();
        }
    }
}
