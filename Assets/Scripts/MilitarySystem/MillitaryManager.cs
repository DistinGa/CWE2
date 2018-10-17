using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using nsWorld;
using nsEventSystem;

namespace nsMilitary
{
    //Класс создаётся в гейм менеджере
    public class MilitaryManager
    {
        public static MilitaryManager Instance;

        MilitaryManager_Ds _MilitaryManagerData;

        private MilitaryManager()
        {
            Instance = this;

            GameEventSystem.Instance.Subscribe(GameEventSystem.MyEventsTypes.ProduceNewMilitaryUnit, ProduceNewMilitaryUnit);
        }

        public void CreateMilitaryManager()
        {
            if (Instance == null)
                new MilitaryManager();
        }

        public SeaPool GetSeaPool(int ind)
        {
            if (ind < 0)
                return null;
            else
                return _MilitaryManagerData._SeaPools[ind];
        }

        public MilitaryBase GetMilitaryBase(int ind)
        {
            if (ind < 0)
                return null;
            else
                return _MilitaryManagerData.MilBases[ind];
        }

        public MilitaryPool GetMainMilPool(int ID)
        {
            return _MilitaryManagerData.MainPools[ID];
        }

        public MilitaryUnit GetMilitaryUnit(int _ID)
        {
            return _MilitaryManagerData.MilitaryUnits[_ID];
        }

        public SystemBody GetSystemBody(int _ID)
        {
            if (_ID < 0 || _ID >= _MilitaryManagerData.BodySystems.Count)
                return null;

            return _MilitaryManagerData.BodySystems[_ID];
        }

        public SystemWeapon GetSystemWeapon(int _ID)
        {
            if (_ID < 0 || _ID >= _MilitaryManagerData.WeaponSystems.Count)
                return null;

            return _MilitaryManagerData.WeaponSystems[_ID];
        }

        public SystemReliability GetSystemReliability(int _ID)
        {
            if (_ID < 0 || _ID >= _MilitaryManagerData.ReliabilitySystems.Count)
                return null;

            return _MilitaryManagerData.ReliabilitySystems[_ID];
        }

        public SystemElectronics GetSystemElectronics(int _ID)
        {
            if (_ID < 0 || _ID >= _MilitaryManagerData.ElectronicsSystems.Count)
                return null;

            return _MilitaryManagerData.ElectronicsSystems[_ID];
        }

        public void NewMilitaryUnit(int authority, int unitType, int unitClass, string unitName, int body, List<int> weapon, List<int> reliability, List<int> electronics)
        {
            int maxKey = _MilitaryManagerData.MilitaryUnits.Keys.Max() + 1;
            _MilitaryManagerData.MilitaryUnits.Add(maxKey, new MilitaryUnit(authority, unitType, unitClass, unitName, body, weapon, reliability, electronics));
        }

        public void NewBodySystem(int authority, string systemName, double initCost, double cost, int capacity, int militaryGeneration, bool investigated, bool active, int upgradeCount, List<int> masterClasses, int armor, int stealth)
        {
            int maxKey = _MilitaryManagerData.BodySystems.Keys.Max() + 1;
            int newVersion = _MilitaryManagerData.BodySystems.Where(d => d.Value.SystemName == systemName).Max((d) => d.Value.Version) + 1;
            _MilitaryManagerData.BodySystems.Add(maxKey, new SystemBody(authority, systemName, newVersion, initCost, cost, capacity, militaryGeneration, investigated, active, upgradeCount, masterClasses, armor, stealth));
        }

        public void NewBodySystem(SystemBody NewSystem)
        {
            int maxKey = _MilitaryManagerData.BodySystems.Keys.Max() + 1;
            int newVersion = _MilitaryManagerData.BodySystems.Where(d => d.Value.SystemName == NewSystem.SystemName).Max((d) => d.Value.Version) + 1;
            NewSystem.Version = newVersion;
            _MilitaryManagerData.BodySystems.Add(maxKey, NewSystem);
        }

        public void NewWeaponSystem(int authority, string systemName, double initCost, double cost, int load, int militaryGeneration, bool investigated, bool active, int upgradeCount, List<int> masterClasses, int hitpoint, int range, List<int> targetClasses)
        {
            int maxKey = _MilitaryManagerData.WeaponSystems.Keys.Max() + 1;
            int newVersion = _MilitaryManagerData.WeaponSystems.Where(d => d.Value.SystemName == systemName).Max((d) => d.Value.Version) + 1;
            _MilitaryManagerData.WeaponSystems.Add(maxKey, new SystemWeapon(authority, systemName, newVersion, initCost, cost, load, militaryGeneration, investigated, active, upgradeCount, masterClasses, hitpoint, range, targetClasses));
        }

        public void NewWeaponSystem(SystemWeapon NewSystem)
        {
            int maxKey = _MilitaryManagerData.WeaponSystems.Keys.Max() + 1;
            int newVersion = _MilitaryManagerData.WeaponSystems.Where(d => d.Value.SystemName == NewSystem.SystemName).Max((d) => d.Value.Version) + 1;
            NewSystem.Version = newVersion;
            _MilitaryManagerData.WeaponSystems.Add(maxKey, NewSystem);
        }

        public void NewReliabilitySystem(int authority, string systemName, double initCost, double cost, int load, int militaryGeneration, bool investigated, bool active, int upgradeCount, List<int> masterClasses, int maneuver, int engine)
        {
            int maxKey = _MilitaryManagerData.ReliabilitySystems.Keys.Max() + 1;
            int newVersion = _MilitaryManagerData.ReliabilitySystems.Where(d => d.Value.SystemName == systemName).Max((d) => d.Value.Version) + 1;
            _MilitaryManagerData.ReliabilitySystems.Add(maxKey, new SystemReliability(authority, systemName, newVersion, initCost, cost, load, militaryGeneration, investigated, active, upgradeCount, masterClasses, maneuver, engine));
        }

        public void NewReliabilitySystem(SystemReliability NewSystem)
        {
            int maxKey = _MilitaryManagerData.ReliabilitySystems.Keys.Max() + 1;
            int newVersion = _MilitaryManagerData.ReliabilitySystems.Where(d => d.Value.SystemName == NewSystem.SystemName).Max((d) => d.Value.Version) + 1;
            NewSystem.Version = newVersion;
            _MilitaryManagerData.ReliabilitySystems.Add(maxKey, NewSystem);
        }

        public void NewElectronicsSystem(int authority, string systemName, double initCost, double cost, int load, int militaryGeneration, bool investigated, bool active, int upgradeCount, List<int> masterClasses, int countermeasures, int radar)
        {
            int maxKey = _MilitaryManagerData.ElectronicsSystems.Keys.Max() + 1;
            int newVersion = _MilitaryManagerData.ElectronicsSystems.Where(d => d.Value.SystemName == systemName).Max((d) => d.Value.Version) + 1;
            _MilitaryManagerData.ElectronicsSystems.Add(maxKey, new SystemElectronics(authority, systemName, newVersion, initCost, cost, load, militaryGeneration, investigated, active, upgradeCount, masterClasses, countermeasures, radar));
        }

        public void NewElectronicsSystem(SystemElectronics NewSystem)
        {
            int maxKey = _MilitaryManagerData.ElectronicsSystems.Keys.Max() + 1;
            int newVersion = _MilitaryManagerData.ElectronicsSystems.Where(d => d.Value.SystemName == NewSystem.SystemName).Max((d) => d.Value.Version) + 1;
            NewSystem.Version = newVersion;
            _MilitaryManagerData.ElectronicsSystems.Add(maxKey, NewSystem);
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

            _MilitaryManagerData.MilitaryUnitsOnTheWay.Add(new UnitOnTheWay(Authority, DestType, DestID, UnitID, Amount, MoveTime, (u) => _MilitaryManagerData.MilitaryUnitsOnTheWay.Remove(u)));

            return true;
        }

        public void BuildMillitaryBase(int RegID, int AuthID)
        {
            Region_Op region = World.TheWorld.GetRegion(RegID);

            if (region.MilitaryBaseID < 0)
            {
                _MilitaryManagerData.MilBases.Add(new MilitaryBase(RegID, AuthID, ModEditor.ModProperties.Instance.DefaultMilBaseCapacity));
                region.RegisterMilBase(_MilitaryManagerData.MilBases.Count - 1);
            }
        }

        //Расчёт времени перемещения между объектами
        public int GetMovementTime(DestinationTypes FromType, int FromID, DestinationTypes DestType, int DestID)
        {
            return 4;
        }

        #region Events Invokes
        void ProduceNewMilitaryUnit(object sender, EventArgs ea)
        {
            ProduceNewUnits_EventArgs e = ea as ProduceNewUnits_EventArgs;
            GetMainMilPool(e.AuthID).AddUnits(e.UnitID, e.Amount);
        }
        #endregion
    }

    public class MilitaryManager_Ds
    {
        public List<MilitaryBase> MilBases;
        public Dictionary<int, MilitaryUnit> MilitaryUnits; //База всех существующих юнитов в игре
        public Dictionary<int, SystemBody> BodySystems; //База всех существующих SystemBody в игре
        public Dictionary<int, SystemWeapon> WeaponSystems; //База всех существующих SystemWeapon в игре
        public Dictionary<int, SystemReliability> ReliabilitySystems; //База всех существующих SystemReliability в игре
        public Dictionary<int, SystemElectronics> ElectronicsSystems; //База всех существующих SystemElectronics в игре
        public List<UnitOnTheWay> MilitaryUnitsOnTheWay; //Юниты в процессе перемещения
        public List<MilitaryPool> MainPools;    //Основные военные пулы контролируемых стран (индекс - _Authorities)
        public List<SeaPool> _SeaPools;

        public MilitaryManager_Ds()
        {
            MilBases = new List<MilitaryBase>();
            _SeaPools = new List<SeaPool>();
        }

    }
}
