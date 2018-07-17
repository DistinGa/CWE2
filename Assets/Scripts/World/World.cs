using System.Collections;
using System.Collections.Generic;
using System.Linq;

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

        public MilitaryPool GetMainMilPool(int ID)
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

        //Расчёт времени перемещения между объектами
        public int GetMovementTime(DestinationTypes FromType, int FromID, DestinationTypes DestType, int DestID)
        {
            return 4;
        }

        //Перемещение военных юнитов между пулами
        public bool MoveMilitaryUnits(int Authority, DestinationTypes FromType, int FromID, DestinationTypes DestType, int DestID, int UnitID, int Amount)
        {
            int MoveTime = GetMovementTime(FromType, FromID, DestType, DestID);
            //Проверка условий возможности перемещения


            switch (FromType)
            {
                case DestinationTypes.MainPool:
                    GetMainMilPool(FromID).AddUnits(UnitID, -Amount);
                    break;
                case DestinationTypes.SeaPool:
                    GetSeaPool(FromID).GetNavy(Authority).AddUnits(UnitID, -Amount);
                    break;
                case DestinationTypes.MilitaryBase:
                    GetMilitaryBase(FromID).AddUnits(UnitID, -Amount);
                    break;
                default:
                    break;
            }

            _WorldData.MilitaryUnitsOnTheWay.Add(new UnitOnTheWay(Authority, DestType, DestID, UnitID, Amount, MoveTime, (u) => _WorldData.MilitaryUnitsOnTheWay.Remove(u)));

            return true;
        }

        public MilitaryUnit GetMilitaryUnit(int _ID)
        {
            return _WorldData.MilitaryUnits[_ID];
        }

        public SystemBody GetSystemBody(int _ID)
        {
            if (_ID < 0 || _ID >= _WorldData.BodySystems.Count)
                return null;

            return _WorldData.BodySystems[_ID];
        }

        public SystemWeapon GetSystemWeapon(int _ID)
        {
            if (_ID < 0 || _ID >= _WorldData.WeaponSystems.Count)
                return null;

            return _WorldData.WeaponSystems[_ID];
        }

        public SystemReliability GetSystemReliability(int _ID)
        {
            if (_ID < 0 || _ID >= _WorldData.ReliabilitySystems.Count)
                return null;

            return _WorldData.ReliabilitySystems[_ID];
        }

        public SystemElectronics GetSystemElectronics(int _ID)
        {
            if (_ID < 0 || _ID >= _WorldData.ElectronicsSystems.Count)
                return null;

            return _WorldData.ElectronicsSystems[_ID];
        }

        public void NewMilitaryUnit(int authority, int unitType, int unitClass, string unitName, int body, int weapon1, int weapon2, int reliability, int electronics)
        {
            int maxKey = _WorldData.MilitaryUnits.Keys.Max() + 1;
            _WorldData.MilitaryUnits.Add(maxKey, new MilitaryUnit(authority, unitType, unitClass, unitName, body, weapon1, weapon2, reliability, electronics));
        }

        public void NewBodySystem(int authority, string systemName, double initCost, double cost, int capacity, int militaryGeneration, bool investigated, bool active, int upgradeCount, List<int> masterClasses, int armor, int stealth)
        {
            int maxKey = _WorldData.BodySystems.Keys.Max() + 1;
            int newVersion = _WorldData.BodySystems.Where(d => d.Value.SystemName == systemName).Max((d) => d.Value.Version) + 1;
            _WorldData.BodySystems.Add(maxKey, new SystemBody(authority, systemName, newVersion, initCost, cost, capacity, militaryGeneration, investigated, active, upgradeCount, masterClasses, armor, stealth));
        }

        public void NewBodySystem(SystemBody NewSystem)
        {
            int maxKey = _WorldData.BodySystems.Keys.Max() + 1;
            int newVersion = _WorldData.BodySystems.Where(d => d.Value.SystemName == NewSystem.SystemName).Max((d) => d.Value.Version) + 1;
            NewSystem.Version = newVersion;
            _WorldData.BodySystems.Add(maxKey, NewSystem);
        }

        public void NewWeaponSystem(int authority, string systemName, double initCost, double cost, int load, int militaryGeneration, bool investigated, bool active, int upgradeCount, List<int> masterClasses, int hitpoint, int range, List<int> targetClasses)
        {
            int maxKey = _WorldData.WeaponSystems.Keys.Max() + 1;
            int newVersion = _WorldData.WeaponSystems.Where(d => d.Value.SystemName == systemName).Max((d) => d.Value.Version) + 1;
            _WorldData.WeaponSystems.Add(maxKey, new SystemWeapon(authority, systemName, newVersion, initCost, cost, load, militaryGeneration, investigated, active, upgradeCount, masterClasses, hitpoint, range, targetClasses));
        }

        public void NewWeaponSystem(SystemWeapon NewSystem)
        {
            int maxKey = _WorldData.WeaponSystems.Keys.Max() + 1;
            int newVersion = _WorldData.WeaponSystems.Where(d => d.Value.SystemName == NewSystem.SystemName).Max((d) => d.Value.Version) + 1;
            NewSystem.Version = newVersion;
            _WorldData.WeaponSystems.Add(maxKey, NewSystem);
        }

        public void NewReliabilitySystem(int authority, string systemName, double initCost, double cost, int load, int militaryGeneration, bool investigated, bool active, int upgradeCount, List<int> masterClasses, int maneuver, int engine)
        {
            int maxKey = _WorldData.ReliabilitySystems.Keys.Max() + 1;
            int newVersion = _WorldData.ReliabilitySystems.Where(d => d.Value.SystemName == systemName).Max((d) => d.Value.Version) + 1;
            _WorldData.ReliabilitySystems.Add(maxKey, new SystemReliability(authority, systemName, newVersion, initCost, cost, load, militaryGeneration, investigated, active, upgradeCount, masterClasses, maneuver, engine));
        }

        public void NewReliabilitySystem(SystemReliability NewSystem)
        {
            int maxKey = _WorldData.ReliabilitySystems.Keys.Max() + 1;
            int newVersion = _WorldData.ReliabilitySystems.Where(d => d.Value.SystemName == NewSystem.SystemName).Max((d) => d.Value.Version) + 1;
            NewSystem.Version = newVersion;
            _WorldData.ReliabilitySystems.Add(maxKey, NewSystem);
        }

        public void NewElectronicsSystem(int authority, string systemName, double initCost, double cost, int load, int militaryGeneration, bool investigated, bool active, int upgradeCount, List<int> masterClasses, int countermeasures, int radar)
        {
            int maxKey = _WorldData.ElectronicsSystems.Keys.Max() + 1;
            int newVersion = _WorldData.ElectronicsSystems.Where(d => d.Value.SystemName == systemName).Max((d) => d.Value.Version) + 1;
            _WorldData.ElectronicsSystems.Add(maxKey, new SystemElectronics(authority, systemName, newVersion, initCost, cost, load, militaryGeneration, investigated, active, upgradeCount, masterClasses, countermeasures, radar));
        }

        public void NewElectronicsSystem(SystemElectronics NewSystem)
        {
            int maxKey = _WorldData.ElectronicsSystems.Keys.Max() + 1;
            int newVersion = _WorldData.ElectronicsSystems.Where(d => d.Value.SystemName == NewSystem.SystemName).Max((d) => d.Value.Version) + 1;
            NewSystem.Version = newVersion;
            _WorldData.ElectronicsSystems.Add(maxKey, NewSystem);
        }

        public bool UpgradeBodySystem(int upgradeType, int ID, int param)
        {
            SystemBody sys = GetSystemBody(ID);

            if (sys == null)
                return false;

            sys = sys.Upgrade(upgradeType);
            NewBodySystem(sys);

            return true;
        }

        public bool UpgradeWeaponSystem(int upgradeType, int ID, int param)
        {
            SystemWeapon sys = GetSystemWeapon(ID);

            if (sys == null)
                return false;

            sys = sys.Upgrade(upgradeType);
            NewWeaponSystem(sys);

            return true;
        }

        public bool UpgradeReliabilitySystem(int upgradeType, int ID, int param)
        {
            SystemReliability sys = GetSystemReliability(ID);

            if (sys == null)
                return false;

            sys = sys.Upgrade(upgradeType);
            NewReliabilitySystem(sys);

            return true;
        }

        public bool UpgradeElectronicsSystem(int upgradeType, int ID, int param)
        {
            SystemElectronics sys = GetSystemElectronics(ID);

            if (sys == null)
                return false;

            sys = sys.Upgrade(upgradeType);
            NewElectronicsSystem(sys);

            return true;
        }
    }

    public class World_Ds
    {
        public int CurrentTurn;
        public int GlobalDevLevel;
        public List<DiplomaticMission> DipMissions;

        //Military
        public List<MilitaryBase> MilBases;
        public Dictionary<int, MilitaryUnit> MilitaryUnits; //База всех существующих юнитов в игре
        public Dictionary<int, SystemBody> BodySystems; //База всех существующих SystemBody в игре
        public Dictionary<int, SystemWeapon> WeaponSystems; //База всех существующих SystemWeapon в игре
        public Dictionary<int, SystemReliability> ReliabilitySystems; //База всех существующих SystemReliability в игре
        public Dictionary<int, SystemElectronics> ElectronicsSystems; //База всех существующих SystemElectronics в игре
        public List<UnitOnTheWay> MilitaryUnitsOnTheWay; //Юниты в процессе перемещения
        public List<MilitaryPool> MainPools;    //Основные военные пулы контролируемых стран (индекс - _Authorities)

        public World_Ds()
        {
            MilBases = new List<MilitaryBase>();
            MilBases.Add(null); //Военная база с инддексом 0 - отсутствующая.
        }
    }
}
