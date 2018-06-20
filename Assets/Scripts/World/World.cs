using System.Collections;
using System.Collections.Generic;

namespace World
{
    public class World_Op
    {
        public static World_Op TheWorld;

        World_Ds _WorldData;
        List<SeaPool> _SeaPools;
        List<string> _Authorities;  //Нулевая считается нейтральной
        Dictionary<int, Region_Op> _Regions;


        private World_Op()
        {
            TheWorld = this;

            _SeaPools = new List<SeaPool>();
            _SeaPools.Add(null);    //Морской пул с индексом 0 пустой.
        }

        public void CreateWorld()
        {
            if (TheWorld == null)
                new World_Op();
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

        public Region_Op GetRegion(int ind)
        {
            return _Regions[ind];
        }
    }

    public class World_Ds
    {
        public int CurrentTurn;
        public int GlobalDevLevel;
        public List<MilitaryBase> MilBases;
        public List<MilitaryUnit> MilitaryUnits;
    }

    public class SeaPool
    {

    }

    public class MilitaryBase
    {

    }

    public class MilitaryUnit
    {

    }
}
