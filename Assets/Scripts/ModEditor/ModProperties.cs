using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace ModEditor
{
    [System.Serializable]
    public class ModProperties
    {
        public static ModProperties Instance;

        private LocalizedDictionary<nsCombat.WarPhase> _LocalizedWarPhasesNames;    //Для кеширования

        public const float YEAR_TURNS_COUNT = 48; //Количество ходов в году (float, потому что на это число чаще всего будут делить целые числа)

        [Tooltip("Время одного хода в секундах")]
        public int TickInterval;    //Время одного хода в секундах.
        [Tooltip("Количество лет игры, после которого игра заканчивается")]
        public int GameYearsCount;  //Количество лет игры, после которого игра заканчивается

        [Tooltip("Список политических режимов (В списке содержатся индексы подсистемы локализации)")]
        public List<string> Authorities;  //Список политических режимов (-1 - нейтральный). (В списке содержатся индексы подсистемы локализации)
        [Tooltip("Иконки политических режимов")]
        public List<Sprite> AuthorityIcons; //Иконки политических режимов. Индекс соответствует Authority.
        [Tooltip("Индекс названия нейтрального режима в подсистеме локализации")]
        public string NeutralRegimeNameID;    //Индекс названия нейтрального режима в подсистеме локализации.
        [Tooltip("Иконка нейтрального режима")]
        public Sprite NeutralRegimeIcon;    //Иконка нейтрального режима.
        [Tooltip("Список контролируемых регионов")]
        public List<int> ControlledRegions;  //Список контролируемых регионов. Индекс соответствует Authority.
        [Tooltip("Список неизменяемых свойств регионов")]
        [DictionaryDrawerSettings(KeyLabel = "RegID", ValueLabel = "Region Properties")]
        public Dictionary<int, Region_Prop> Regions;        //Список неизменяемых свойств регионов
        [Tooltip("Список изменяемых свойств регионов")]
        public Dictionary<int, nsWorld.Region_Ds> Regions_Originals;        //Список изменяемых свойств регионов

        //Ежегодный прирост GNP в неконтролируемых странах.
        public int GNP_Neutral_Min, GNP_Neutral_Max, GNP_HighDevLevel_Min, GNP_HighDevLevel_Max, GNP_LowDevLevel_Min, GNP_LowDevLevel_Max;  //Интервалы изменения GNP нейтральных стран
        [Tooltip("Максимальное значение Prosperity (+/-)")]
        public int ProspMaxValue;    //+/- для Radicalizm и Prosperity параметра DevLevel
        [Tooltip("Сколько процентов добавлять в нацфонд за один уровень благосостояния")]
        [Range(0, 100)]
        public double ProsperityAdditionToNatFund;  //Сколько процентов добавлять в нацфонд за один уровень благосостояния (0 - 100)
        [Tooltip("Еженедельный рост частного сектора (0-1%)")]
        [Range(0, 1)]
        public float PrivateWeeklyGrow;    //Еженедельный рост частного сектора (0-1%)
        [Tooltip("Еженедельный рост загруженного частного сектора (0-1%)")]
        [Range(0, 1)]
        public float PrivateLoadedWeeklyGrow;    //Еженедельный рост загруженного частного сектора (0-1%)
        public float PrivateFactor;         //Во сколько раз дороже постройка юнитов за счёт частного сектора
        public List<string> MilitaryUnitClasses;   //Helicopter / Tank / Submarine ...

        //Посольства
        [Header("Ambassy")]
        [Tooltip("Список всех существующих в игре дип. миссий")]
        public List<nsEmbassy.DiplomaticMission> DipMissionsList;   //Список всех существующих в игре дип. миссий
        [Tooltip("Наименования уровней посольства")]
        public List<string> EmbassyLevelsNames;     //Наименования уровней посольства
        [Tooltip("Количество шпионских сетей по уровням посольств")]
        public List<int> EmbassyLevelSizes;      //Количество шпионских сетей по уровням посольств
        [Tooltip("Стоимость первого апгрейда посольства")]
        public float InitEmbassyUpgradeCost;         //Стоимость первого апгрейда посольства
        [Tooltip("Стоимость первого апгрейда шпионской сети")]
        public float InitSpyNetUpgradeCost;         //Стоимость первого апгрейда шпионской сети
        [Tooltip("Начальный процент успешности сети")]
        public int InitSpyNetSuccess;             //Начальный процент успешности сети
        [Tooltip("Во сколько раз увеличивается стоимость следующего апгрейда")]
        public float EmbassyUpgradeCostFactor;       //Во сколько раз увеличивается стоимость следующего апгрейда
        [Tooltip("На сколько процентов увеличивается успех миссии сети")]
        public float SpyNetSuccessUpgradePercent;   //На сколько процентов увеличивается успех миссии сети
        [Tooltip("На сколько процентов увеличивается скорость выполнения миссии сети")]
        public int SpyNetSpeedUpgradePercent;     //На сколько процентов увеличивается скорость выполнения миссии сети
        [Tooltip("Время, на которое блокируется вражеская сеть при контр-шпионаже")]
        public int SpyNetCounterEspionageDelayTime; //Время, на которое блокируется вражеская сеть при контр-шпионаже

        //Парламент
        [Header("Parliament")]
        [Tooltip("Список разновидностей политических партий")]
        public List<PoliticParty_Props> PoliticParties = new List<PoliticParty_Props>();  //Список разновидностей политических партий (во всех регионах такой набор партий)
        //public List<PoliticLaw_Props> PoliticLaws;        //Список всех возможных в игре законов
        [Tooltip("Ежегодный прирост популярности партии")]
        [Range(0, 100)]
        public float AnnualPartyPopularityGain;    //Ежегодный прирост популярности партии (в неконтролируемом регионе меняется раз в год, в контролируемом - каждый ход на кратное (1/48) значение) (1 - 100)
        [Tooltip("Ежегодный прирост популярности групп радикалов")]
        [Range(0, 100)]
        public float AnnualRadicalsPopularityGain;    //Ежегодный прирост популярности групп радикалов (1 - 100)
        [Tooltip("Долг в нац фонде, который считается за 100% при расчёте скорости роста партии, зависящей от него")]
        public float RelativeNatFundToPartyGrow;    //Долг в нац фонде, который считается за 100% при расчёте скорости роста партии, зависящей от него
        [Tooltip("Время принятия закона лидирующей партией (ходы)")]
        public int PassingLawTime1;                 //Время принятия закона лидирующей партией (ходы)
        [Tooltip("Время принятия закона второй лидирующей партией (ходы)")]
        public int PassingLawTime2;                 //Время принятия закона второй лидирующей партией (ходы)
        [Tooltip("Время принятия закона остальными партиями (ходы)")]
        public int PassingLawTime_;                 //Время принятия закона остальными партиями (ходы)

        //Military
        [Header("Military")]
        [Tooltip("Классы юнитов (танк, истребитель, пехота и тд.)")]
        public List<nsMilitary.UnitClass> UnitClasses;  //Классы юнитов (танк, истребитель, пехота и тд.)
        public Dictionary<int, nsMilitary.SystemBody> BodySystems;
        public Dictionary<int, nsMilitary.SystemWeapon> WeaponSystems;
        public Dictionary<int, nsMilitary.SystemReliability> ReliabilitySystems;
        public Dictionary<int, nsMilitary.SystemElectronics> ElectronicsSystems;
        public Dictionary<int, Sprite> BodyImages;
        public Dictionary<int, Sprite> WeaponImages;
        public Dictionary<int, Sprite> ReliabilityImages;
        public Dictionary<int, Sprite> ElectronicsImages;
        public Dictionary<int, nsMilitary.IMilitaryUnit> MilitaryUnits;
        public Dictionary<int, nsMilitary.MilitaryPool> MainPools;    //Домашние пулы (индекс - индекс региона)
        public Dictionary<int, nsMilitary.SeaPool> SeaPools;
        [Tooltip("Замена нейтральных юнитов уникальными")]
        public List<MilitaryUnitsReplacement> MilitaryUnitsReplacement;

        [Space(10)]
        [Tooltip("Лимит выделенных средств на производство одного юнита (изучение одной системы). (В деньгах)")]
        public double GlobalDevLimit;   // Лимит выделенных средств на производство одного юнита (изучение одной системы). (В деньгах)
        [Tooltip("Общий лимит выделения средств из бюджета на производство/изучение в начале игры")]
        public float InitialDevLimit;   // Общий лимит выделения средств из бюджета на производство/изучение в начале игры
        [Tooltip("Увеличение стоимости производства военной системы при апгрейде (коэффициент)")]
        public float MilitarySystemCostIncreasePerUpgrade;  //Увеличение стоимости производства военной системы при апгрейде (коэффициент)
        [Tooltip("Снижение стоимости производства военной системы от апгрейда типа 4 (коэффициент)")]
        public float MilitarySystemCostDecreaseByUpgrade;   //Снижение стоимости производства военной системы от апгрейда типа 4
        [Tooltip("Увеличение вместимости (body) или уменьшение занимаемого места других систем при апгрейде")]
        public int MilitarySystemCapacityUpgrade;   //Увеличение вместимости (body) или уменьшение занимаемого места других систем при апгрейде
        [Tooltip("На сколько увеличивается один из двух параметров системы при апгрейде")]
        public int MilitarySystemParamIncreaseByUpgrade;    //На сколько увеличивается один из двух параметров системы при апгрейде.
        [Tooltip("Land / Sea / Air")]
        public List<string> MilitaryUnitTypes;     //Land / Sea / Air
        [Tooltip("Предел своего влияния в нейтральной стране для возможности построить базу")]
        public int SelfInflToBuildBase;     //Предел своего влияния в нейтральной стране для возможности построить базу
        [Tooltip("Вместимость новой базы")]
        public int DefaultMilBaseCapacity;  //Вместимость новой базы
        [Tooltip("Стоимость новой базы")]
        public float InitMilBaseCost;       //Стоимость новой базы
        [Tooltip("На сколько увеличивается вместимость военной базы при апгрейде (абсолютное значение)")]
        public int UpgradeMilBaseCapacity;  //На сколько увеличивается вместимость военной базы при апгрейде
        [Tooltip("Увеличение стоимости апгрейда военной базы (коэффициент)")]
        public float MilBaseUpgradeCostFactor;  //Увеличение стоимости апгрейда военной базы (коэффициент)

        //Combat
        [Header("Combat")]
        [Tooltip("Названия фаз войны (индекс в системе локализации)")]
        public Dictionary<nsCombat.WarPhase, string> WarPhasesNames;   // Названия фаз войны. Реальные значения в игре берутся из свойства LocalizedWarPhasesNames.
        [Tooltip("Список всех особенностей рельефа")]
        public Dictionary<int, nsCombat.ReliefProperties> ReliefProperties; // Список всех особенностей рельефа
        [Tooltip("Особенности рельефа для региона и фазы войны")]
        public Dictionary<int, Dictionary<nsCombat.WarPhase, int>> RegPhaseReliefProperties;  //Особенности рельефа для региона и фазы войны (<RegionID, <WarPhase, ReliefPropertiesID>>)
        [Tooltip("Сколько Supply отнимается при выстреле")]
        public int FireCost;    //Сколько Supply отнимается при выстреле
        [Tooltip("Размер поля боя, и центральная облать (количество линий для каждой стороны)")]
        public int CombatArea, CenterCombatArea;    //Размер поля боя, и центральная облать (количество линий для каждой стороны)
        [Tooltip("Штраф к морали за агрессию")]
        public int AggressorMoralPenalty;           //Штраф к морали за агрессию
        [Tooltip("Штраф к морали за проигрыш в фазе войны")]
        public int RetreatMoralPenalty;           //Штраф к морали за проигрыш в фазе войны
        [Tooltip("Количество ходов без военных действий, после которого начинается контратака государства")]
        public int NonePhaseTurns;              // Количество ходов без военных действий, после которого начинается контратака государства.
        [Tooltip("Скорость перемещения военных юнитов между пулами")]
        public float UnitMovementSpeed;     //Скорость перемещения военных юнитов между пулами
        [Tooltip("Стоимость перемещения военных юнитов между пулами ($ за ход)")]
        public int UnitMovementCost;        //Стоимость перемещения военных юнитов между пулами ($ за ход)

        public Sprite GetRegimeIcon(int AuthorityID)
        {
            if (AuthorityID == -1 || AuthorityID >= AuthorityIcons.Count)
                return NeutralRegimeIcon;

            return AuthorityIcons[AuthorityID];
        }

        public string GetRegimeName(int AuthorityID)
        {
            if (AuthorityID == -1 || AuthorityID >= AuthorityIcons.Count)
                return Assets.SimpleLocalization.LocalizationManager.Localize(NeutralRegimeNameID);

            return Assets.SimpleLocalization.LocalizationManager.Localize(Authorities[AuthorityID]);
        }

        // Кэшированный словарь названий фаз войны
        public LocalizedDictionary<nsCombat.WarPhase> LocalizedWarPhasesNames   // Локализованные названия фаз войны
        {
            get
            {
                if (_LocalizedWarPhasesNames == null)
                    _LocalizedWarPhasesNames = new LocalizedDictionary<nsCombat.WarPhase>(WarPhasesNames);

                return _LocalizedWarPhasesNames;
            }
        }

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

    public class MilitaryUnitsReplacement
    {
        public int UnitID;
        public int RegID;
        public int UnitClass;
        public int Generation;
    }
}
