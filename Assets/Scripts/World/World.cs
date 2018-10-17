using System;
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
        List<string> _Authorities;  //Нулевая считается нейтральной
        Dictionary<int, Region_Op> _Regions;
        List<RegionController> _RegionControllers;  //index - Authority

        private World()
        {
            TheWorld = this;

            GameEventSystem.Instance.Subscribe(GameEventSystem.MyEventsTypes.TurnEvents, OnTurn);
            GameEventSystem.Instance.Subscribe(GameEventSystem.MyEventsTypes.TurnEvents, OnDipMissionComplete);
        }

        ~World()
        {
            GameEventSystem.Instance.UnSubscribe(GameEventSystem.MyEventsTypes.TurnEvents, OnTurn);
             GameEventSystem.Instance.UnSubscribe(GameEventSystem.MyEventsTypes.TurnEvents, OnDipMissionComplete);
       }

        public void CreateWorld()
        {
            if (TheWorld == null)
                new World();
        }

        void OnTurn(object sender, EventArgs e)
        {
        }

        void OnDipMissionComplete(object sender, EventArgs e)
        {
            nsEmbassy.SpyNet sn = sender as nsEmbassy.SpyNet;
            nsEmbassy.Embassy.EmbassiesList[sn.RegionID][sn.AuthorityID].MissionComplete(sn);
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

    public class World_Ds
    {
        public int CurrentTurn;
        public int GlobalDevLevel;

        public World_Ds()
        {
        }
    }
}
