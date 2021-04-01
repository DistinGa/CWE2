using System;
using System.Collections;
using System.Collections.Generic;
using nsEventSystem;
using ModEditor;

namespace nsEmbassy
{
    public class Embassy
    {
        const int MANUAL_DIPFOCUS = -1;

        public Action OnChange;

        public List<SpyNet> SpyNets;

        private int regionID, authorityID;
        private int embassyLevel;    //уровень/размер посольства
        private int dipFocusID;      //ИД дипломатического фокуса (ModProperties.DipMissionsList)


        public int EmbassyLevel
        {
            get
            {
                return embassyLevel;
            }

            private set //EmbassyLevel  нельзя менять произвольно, только через Upgrade
            {
                embassyLevel = value;
            }
        }

        public int DipFocusID
        {
            get
            {
                return dipFocusID;
            }

            set
            {
                dipFocusID = value;
                if (value == MANUAL_DIPFOCUS)
                {
                    foreach (var item in SpyNets)
                        item.AbortMission();
                }
            }
        }

        public int AuthorityID
        {
            get { return authorityID; }
        }

        public int RegionID
        {
            get { return regionID; }
        }

        public double EmbassyUpgradeCost
        {
            get { return GameManager.GM.GameProperties.InitEmbassyUpgradeCost * Math.Pow(GameManager.GM.GameProperties.EmbassyUpgradeCostFactor, EmbassyLevel); }
        }

        /// <summary>
        /// Посольство на последнем уровне развития
        /// </summary>
        public bool HighestEmbassy
        {
            get { return EmbassyLevel + 1 >= GameManager.GM.GameProperties.EmbassyLevelSizes.Count; }
        }

        public Embassy(int pAuthorityID, int pRegionID, int pEmbassyLevel, int pDipFocusID)
        {
            authorityID = pAuthorityID;
            regionID = pRegionID;
            embassyLevel = pEmbassyLevel;
            dipFocusID = pDipFocusID;

            int cnt = GameManager.GM.GameProperties.EmbassyLevelSizes[EmbassyLevel];
            for (int i = 0; i < cnt; i++)
            {
                SpyNet newSN = new SpyNet(pRegionID, pAuthorityID);
                SetNewMissionByDipFocus(newSN);
                SpyNets.Add(newSN);
            }

            nsWorld.World.TheWorld.Embassies[pRegionID].Insert(pAuthorityID, this);
        }

        public Embassy(int AuthorityID, int RegionID): this(AuthorityID, RegionID, 0, 0){}

        /// <summary>
        /// Апгрейд посольства.
        /// </summary>
        /// <param name="SourceID">0 - из нацфонда; 1 - из престижа</param>
        public void Upgrade(int SourceID = 0)
        {
            if (EmbassyLevel + 1 >= ModProperties.Instance.EmbassyLevelSizes.Count)
                return; //Посольство и так на последнем уровне

            RegionController RC = nsWorld.World.TheWorld.GetRegionController(AuthorityID);
            if (!RC.PayCount(SourceID, EmbassyUpgradeCost))
                return; //Не хватает престижа.

            EmbassyLevel++;
            //Добавление сетей
            int cnt = ModProperties.Instance.EmbassyLevelSizes[EmbassyLevel] - SpyNets.Count;
            for (int i = 0; i < cnt; i++)
            {
                SpyNet newSN = new SpyNet(RegionID, AuthorityID);
                SetNewMissionByDipFocus(newSN);
                SpyNets.Add(newSN);
            }
        }

        /// <summary>
        /// Назначение новой миссии в соответствии с установленным дипломатическим фокусом.
        /// </summary>
        /// <param name="spyNet"></param>
        public void SetNewMissionByDipFocus(SpyNet spyNet)
        {
            
        }

        bool CheckDipFocusCondition()
        {
            return true;
        }

        /// <summary>
        /// Проверка достижения цели фокуса.
        /// </summary>
        /// <param name="pSpyNet"></param>
        public void MissionComplete(SpyNet pSpyNet)
        {
            if (DipFocusID == 0)    //фокус "Manual"
                pSpyNet.RestartDipMission();
            else
            {
                if (CheckDipFocusCondition())
                    DipFocusID = 0;
                else
                    SetNewMissionByDipFocus(pSpyNet);
            }
        }
    }

    public class SpyNet
    {
        const int NULLDIPMISSION = -1;

        public Action OnChange;

        private int regionID, authorityID;
        private int successLevel;
        private int speedLevel;
        private int dipMissionID;
        private int success;
        private float delayTime;  //Время блокировки сети контр-шпионажем

        private float restTime;  //Остаточная продолжительность миссии в ходах

        public SpyNet(int RegionID, int AuthorityID)
        {
            regionID = RegionID;
            authorityID = AuthorityID;
            successLevel = 0;
            speedLevel = 0;
            dipMissionID = NULLDIPMISSION;
            success = GameManager.GM.GameProperties.InitSpyNetSuccess;

            GameEventSystem.Subscribe(GameEventSystem.MyEventsTypes.TurnActions, OnTurn);
        }

        public int AuthorityID
        {
            get { return authorityID; }
        }

        public int RegionID
        {
            get { return regionID; }
        }

        public int SuccessLevel
        {
            get
            {
                return successLevel;
            }

            private set
            {
                if (successLevel != value)
                {
                    successLevel = value;
                    InvokeOnChange();
                }
            }
        }

        public int SpeedLevel
        {
            get
            {
                return speedLevel;
            }

            private set
            {
                if (speedLevel != value)
                {
                    speedLevel = value;
                    InvokeOnChange();
                }
            }
        }

        public int DipMissionID
        {
            get
            {
                return dipMissionID;
            }

            private set
            {
                if (dipMissionID != value)
                {
                    dipMissionID = value;
                    InvokeOnChange();
                }
            }
        }

        public float DelayTime
        {
            get { return delayTime; }

            private set
            {
                if (delayTime != value)
                {
                    delayTime = value;
                    InvokeOnChange();
                }
            }
        }

        public DiplomaticMission DipMission
        {
            get
            {
                return GameManager.GM.GameProperties.DipMissionsList[dipMissionID];
            }
        }

        public double SuccessUpgradeCost
        {
            get { return GameManager.GM.GameProperties.InitSpyNetUpgradeCost * Math.Pow(GameManager.GM.GameProperties.EmbassyUpgradeCostFactor, SuccessLevel); }
        }

        public double SpeedUpgradeCost
        {
            get { return GameManager.GM.GameProperties.InitSpyNetUpgradeCost * Math.Pow(GameManager.GM.GameProperties.EmbassyUpgradeCostFactor, SpeedLevel); }
        }

        void InvokeOnChange()
        {
            if (OnChange != null)
                OnChange();
        }

        /// <summary>
        /// Апгрейд успешности сети.
        /// </summary>
        /// <param name="SourceID">0 - из нацфонда; 1 - из престижа</param>
        public void UpgradeSuccessLevel(int SourceID = 0)
        {
            RegionController RC = nsWorld.World.TheWorld.GetRegionController(AuthorityID);
            if (!RC.PayCount(SourceID, SuccessUpgradeCost))
                return; //Не хватает престижа.

            SuccessLevel++;
            success += (int)((100 - success) * GameManager.GM.GameProperties.SpyNetSuccessUpgradePercent); //асимптотическое приближение к 100%
        }

        /// <summary>
        /// Апгрейд скорости сети
        /// </summary>
        /// <param name="SourceID">0 - из нацфонда; 1 - из престижа</param>
        public void UpgradeSpeedLevel(int SourceID = 0)
        {
            RegionController RC = nsWorld.World.TheWorld.GetRegionController(AuthorityID);
            if (!RC.PayCount(SourceID, SpeedUpgradeCost))
                return; //Не хватает престижа.

            SpeedLevel++;
        }

        public void StartDipMission(int pMissionID)
        {
            if (pMissionID < 0 || pMissionID >= GameManager.GM.GameProperties.DipMissionsList.Count)
                return;

            dipMissionID = pMissionID;
            restTime = DipMission.LifeTime;
        }

        public void RestartDipMission()
        {
            StartDipMission(dipMissionID);
        }

        void OnTurn(object sender, EventArgs e)
        {
            //Блокировка сети контр-шпионажем
            if (DelayTime > 0)
            {
                DelayTime -= 1f + (speedLevel * GameManager.GM.GameProperties.SpyNetSpeedUpgradePercent) * 0.01f;
                return;
            }

            restTime -= 1f + (speedLevel * GameManager.GM.GameProperties.SpyNetSpeedUpgradePercent)*0.01f;
            if (restTime <= 0)
            {
                ExecuteMission();
            }
        }

        void ExecuteMission()
        {
            if (GameManager.GM.Randomizer.GetRndRange(0, 100) <= success)
            {
                GameEventSystem.InvokeEvents(DipMission.EventType, DipMission.EventArgs);
                //Отправка события о выполнении миссии
                GameEventSystem.InvokeEvents(GameEventSystem.MyEventsTypes.SpyNetCompletesDipMission, this);
            }
        }

        public void AbortMission()
        {
            if (DipMissionID != NULLDIPMISSION)
            {
                DipMissionID = NULLDIPMISSION;
                GameEventSystem.InvokeEvents(GameEventSystem.MyEventsTypes.SpyNetCompletesDipMission, this);
            }
        }

        /// <summary>
        /// Блокировка сети контр-шпионажем
        /// </summary>
        /// <param name="pEnemySpyNet"></param>
        void DelaySpyNet(SpyNet pEnemySpyNet)
        {
            DelayTime = GameManager.GM.GameProperties.SpyNetCounterEspionageDelayTime * (1f + (pEnemySpyNet.SpeedLevel - SpeedLevel) * GameManager.GM.GameProperties.SpyNetSpeedUpgradePercent * 0.01f);
        }
    }
}
