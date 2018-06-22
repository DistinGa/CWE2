using System.Collections;
using System.Collections.Generic;

namespace World
{
    public class World
    {
        public static World TheWorld;

        World_Ds _WorldData;
        List<SeaPool> _SeaPools;
        List<string> _Authorities;  //Нулевая считается нейтральной
        Dictionary<int, Region_Op> _Regions;


        private World()
        {
            TheWorld = this;

            _SeaPools = new List<SeaPool>();
            _SeaPools.Add(null);    //Морской пул с индексом 0 - отсутствующий.
            GameEventSystem.Instance.SubscribeOnTurn(OnTurn);
        }

        ~World()
        {
            GameEventSystem.Instance.SubscribeOnTurn(OnTurn, false);
        }

        public void CreateWorld()
        {
            if (TheWorld == null)
                new World();
        }

        void OnTurn()
        {
            //Удаление выполненных дипломатических миссий
            DiplomaticMission dm;
            for (int i = _WorldData.DipMissions.Count - 1; i >= 0; i--)
            {
                dm = _WorldData.DipMissions[i];
                if (dm.LifeTime <= 0)
                    _WorldData.DipMissions.RemoveAt(i);
            }
        }

        public SeaPool GetSeaPool(int ind)
        {
            if (ind == 0)
                return null;
            else
                return _SeaPools[ind];
        }

        public MilitaryBase GetMilitaryBase(int ind)
        {
            if (ind == 0)
                return null;
            else
                return _WorldData.MilBases[ind];
        }

        public MilitariPool GetMainMilPool(int ID)
        {
            return _WorldData.MainPools[ID];
        }

        public Region_Op GetRegion(int ind)
        {
            return _Regions[ind];
        }

        public void BuildMillitaryBase(int RegID, int AuthID)
        {
            if (GetRegion(RegID).MilitaryBaseID == 0)
            {
                _WorldData.MilBases.Add(new MilitaryBase(RegID, AuthID, ModEditor.ModProperties.Instance.DefaultMilBaseCapacity));
                GetRegion(RegID).RegisterMilBase(_WorldData.MilBases.Count - 1);
            }
        }

        public bool MoveMilUnits(DestinationTypes FromType, int FromID, DestinationTypes DestType, int DestID, int UnitID, int Amount)
        {
            //Проверка условий возможности перемещения


            switch (FromType)
            {
                case DestinationTypes.MainPool:
                    GetMainMilPool(FromID).AddUnits(UnitID, -Amount);
                    break;
                case DestinationTypes.SeaPool:
                    GetSeaPool(FromID).AddUnits(UnitID, -Amount);
                    break;
                case DestinationTypes.MilitaryBase:
                    GetMilitaryBase(FromID).AddUnits(UnitID, -Amount);
                    break;
                default:
                    break;
            }

            _WorldData.MilitaryUnitsOnTheWay.Add(new UnitOnTheWay(DestType, DestID, UnitID, Amount, 0, (u) => _WorldData.MilitaryUnitsOnTheWay.Remove(u)));

            return true;
        }
    }

    public class World_Ds
    {
        public int CurrentTurn;
        public int GlobalDevLevel;
        public List<MilitaryBase> MilBases;
        public Dictionary<int, MilitaryUnit> MilitaryUnits; //База всех существующих юнитов в игре
        public List<UnitOnTheWay> MilitaryUnitsOnTheWay; //Юниты в процессе перемещения
        public List<MilitariPool> MainPools;    //Основные военные пулы контролируемых стран (индекс - _Authorities)
        public List<DiplomaticMission> DipMissions;

        public World_Ds()
        {
            MilBases = new List<MilitaryBase>();
            MilBases.Add(null); //Военная база с инддексом 0 - отсутствующая.
        }
    }

    public class MilitaryUnit
    {

    }
}
