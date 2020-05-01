using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using nsEventSystem;

namespace nsWorld
{
    //Класс создаётся в гейм менеджере
    public class World
    {
        public static World TheWorld;

        World_Ds _WorldData;
        Dictionary<int, Region_Op> _Regions;
        List<RegionController> _RegionControllers;  //index - Authority

        private World()
        {
            TheWorld = this;
            _WorldData = new World_Ds();

            GameEventSystem.Subscribe(GameEventSystem.MyEventsTypes.TurnActions, OnTurn);
            GameEventSystem.Subscribe(GameEventSystem.MyEventsTypes.SpyNetCompletesDipMission, OnDipMissionComplete);
            GameEventSystem.Subscribe(GameEventSystem.MyEventsTypes.AbortPartyLawInRegion, OnAbortPartyLawInRegion);
        }

        ~World()
        {
            GameEventSystem.UnSubscribe(GameEventSystem.MyEventsTypes.TurnActions, OnTurn);
            GameEventSystem.UnSubscribe(GameEventSystem.MyEventsTypes.SpyNetCompletesDipMission, OnDipMissionComplete);
            GameEventSystem.UnSubscribe(GameEventSystem.MyEventsTypes.AbortPartyLawInRegion, OnAbortPartyLawInRegion);
        }

        public static void CreateWorld()
        {
            if (TheWorld != null) return;

            new World();
            TheWorld._RegionControllers = new List<RegionController>();
            for (int i = 0; i < ModEditor.ModProperties.Instance.ControlledRegions.Count; i++)
            {
                TheWorld._RegionControllers.Add(new RegionController(i, ModEditor.ModProperties.Instance.ControlledRegions[i]));
            }

            TheWorld.LoadRegions();

        }

        #region События
        void OnTurn(object sender, EventArgs e)
        {
            _WorldData.CurrentTurn++;
        }

        void OnDipMissionComplete(object sender, EventArgs e)
        {
            nsEmbassy.SpyNet sn = sender as nsEmbassy.SpyNet;
            Embassies[sn.RegionID][sn.AuthorityID].MissionComplete(sn);
        }

        void OnAbortPartyLawInRegion(object sender, EventArgs e)
        {
            if (!(e is AbortPartyLawInRegion_EventArgs))
                throw new Exception("Invalid EventArgs");

            var args = e as AbortPartyLawInRegion_EventArgs;

            GetRegion(args.RegID).PartyAbortLaw(args.PartyID);
        }
        #endregion

        #region Свойства
        public Dictionary<int, Region_Op> Regions
        {
            get { return _Regions; }
        }

        public Dictionary<int, List<nsEmbassy.Embassy>> Embassies
        {
            get
            {
                return _WorldData.EmbassiesList;
            }
        }

        // Определение максимальной Military Generation, изученной хотя бы двумя контролируемыми регионами.
        // (Нужно для определения, какие юниты строить в неконтролируемых регионах.)
        public int MaxMilitaryGeneration
        {
            get
            {
                int res = 0;

                SortedList<int, int> _MilGenCounter = new SortedList<int, int>();

                foreach (var item in _RegionControllers)
                {
                    if (!_MilGenCounter.ContainsKey(item.TechMilitary))
                        _MilGenCounter[item.TechMilitary] = 1;
                    else
                        _MilGenCounter[item.TechMilitary]++;
                }

                foreach (var item in _MilGenCounter)
                {
                    if (item.Value >= 2)
                        res = item.Key;
                }

                return res;
            }
        }
        #endregion

        private void LoadRegions()
        {
            for (int i = 0; i < ModEditor.ModProperties.Instance.Regions.Count; i++)
            {
                var reg = ModEditor.ModProperties.Instance.Regions[i];
                _Regions.Add(reg.RegID, new Region_Op(reg.RegID, reg.SeaPoolID, FindRegionControllerForRegID(reg.RegID), ModEditor.ModProperties.Instance.Regions_Originals[i]));
            }
        }

        public RegionController FindRegionControllerForRegID(int RegID)
        {
            return GetRegion(RegID).RegionController;
        }

        public Region_Op GetRegion(int ind)
        {
            return _Regions[ind];
        }

        public RegionController GetRegionController(int AuthorityID)
        {
            return _RegionControllers[AuthorityID];
        }

    }


    ////////////////////////////////////////////
    public class World_Ds:ISavable
    {
        public int CurrentTurn;
        public int GlobalDevLevel;
        public Dictionary<int, List<nsEmbassy.Embassy>> EmbassiesList;    //Key - RegionID, ListIndex - AuthorityID

        public World_Ds()
        {
            EmbassiesList = new Dictionary<int, List<nsEmbassy.Embassy>>();
        }
    }
}
