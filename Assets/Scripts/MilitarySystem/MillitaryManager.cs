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
        public Dictionary<int, UnitClass> UnitClasses;

        MilitaryManager_Ds _MilitaryManagerData;

        private MilitaryManager(MilitaryManager_Ds MilitaryManagerData)
        {
            Instance = this;

            _MilitaryManagerData = MilitaryManagerData;
            UnitClasses = new Dictionary<int, UnitClass>();

            GameEventSystem.Subscribe(GameEventSystem.MyEventsTypes.ProduceNewMilitaryUnit, ProduceNewMilitaryUnit);
        }

        public static void CreateMilitaryManager(MilitaryManager_Ds MilitaryManagerData)
        {
            if (Instance == null)
                new MilitaryManager(MilitaryManagerData);
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

        /// <summary>
        /// Возвращает домашний военный пул
        /// </summary>
        /// <param name="ID">индекс региона</param>
        /// <returns></returns>
        public MilitaryPool GetMainMilPool(int ID)
        {
            if(ID == -1)
                return null;
            else
                return _MilitaryManagerData.MainPools[ID];
        }

        public IMilitaryUnit GetMilitaryUnit(int _ID)
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

        public float GetUnitFirepower(int militaryUnitID)
        {
            return GetUnitFirepower(GetMilitaryUnit(militaryUnitID));
        }

        public float GetUnitFirepower(IMilitaryUnit militaryUnit)
        {
            float _res = 0;
            int fc = 0;

            foreach (var item in militaryUnit.AvailableWeapons)
            {
                _res += 0.7f * militaryUnit.GetHitPoints(item);
                _res += 0.3f * militaryUnit.GetRange(item);
                fc += militaryUnit.GetFireCost(item);
            }

            _res = 0.35f * _res * militaryUnit.Supply / fc; // Количество выстрелов умноженное на огневую мощь (_res - кол-во выстрелов).

            _res += 0.1f * militaryUnit.Maneuver;
            _res += 0.07f * militaryUnit.Countermeasures;
            _res += 0.25f * militaryUnit.Armor;
            _res += 0.1f * militaryUnit.Radar;
            _res += 0.09f * militaryUnit.Stealth;
            _res += 0.04f * militaryUnit.Engine;

            return _res;
        }

        public int NewMilitaryUnit(IMilitaryUnit militaryUnit)
        {
            int maxKey = _MilitaryManagerData.MilitaryUnits.Count == 0? 0: _MilitaryManagerData.MilitaryUnits.Keys.Max() + 1;
            _MilitaryManagerData.MilitaryUnits.Add(maxKey, militaryUnit);
            return maxKey;
        }

        public int NewMilitaryUnit(int authority, UnitType unitType, int unitClass, string unitName, int body, List<int> weapon, List<int> reliability, List<int> electronics)
        {
            return NewMilitaryUnit(new MilitaryUnit(authority, unitType, unitClass, unitName, body, weapon, reliability, electronics));
        }

        public void NewBodySystem(int authority, string systemName, double initCost, double cost, int capacity, int militaryGeneration, bool investigated, bool active, int upgradeCount, List<int> masterClasses, int armor, int stealth)
        {
            int maxKey = _MilitaryManagerData.BodySystems.Count == 0? 0: _MilitaryManagerData.BodySystems.Keys.Max() + 1;
            int newVersion = _MilitaryManagerData.BodySystems.Where(d => d.Value.SystemName == systemName).Max((d) => d.Value.Version) + 1;
            _MilitaryManagerData.BodySystems.Add(maxKey, new SystemBody(authority, systemName, newVersion, initCost, cost, capacity, militaryGeneration, investigated, active, upgradeCount, masterClasses, armor, stealth));
        }

        public void NewBodySystem(SystemBody NewSystem)
        {
            int maxKey = _MilitaryManagerData.BodySystems.Count == 0? 0: _MilitaryManagerData.BodySystems.Keys.Max() + 1;
            int newVersion = _MilitaryManagerData.BodySystems.Where(d => d.Value.SystemName == NewSystem.SystemName).Max((d) => d.Value.Version) + 1;
            NewSystem.Version = newVersion;
            _MilitaryManagerData.BodySystems.Add(maxKey, NewSystem);
        }

        public void NewWeaponSystem(int authority, string systemName, double initCost, double cost, int load, int militaryGeneration, bool investigated, bool active, int upgradeCount, List<int> masterClasses, int hitpoint, int range, List<int> targetClasses, int fireCost)
        {
            int maxKey = _MilitaryManagerData.WeaponSystems.Count == 0? 0: _MilitaryManagerData.WeaponSystems.Keys.Max() + 1;
            int newVersion = _MilitaryManagerData.WeaponSystems.Where(d => d.Value.SystemName == systemName).Max((d) => d.Value.Version) + 1;
            _MilitaryManagerData.WeaponSystems.Add(maxKey, new SystemWeapon(authority, systemName, newVersion, initCost, cost, load, militaryGeneration, investigated, active, upgradeCount, masterClasses, hitpoint, range, targetClasses, fireCost));
        }

        public void NewWeaponSystem(SystemWeapon NewSystem)
        {
            int maxKey = _MilitaryManagerData.WeaponSystems.Count == 0? 0: _MilitaryManagerData.WeaponSystems.Keys.Max() + 1;
            int newVersion = _MilitaryManagerData.WeaponSystems.Where(d => d.Value.SystemName == NewSystem.SystemName).Max((d) => d.Value.Version) + 1;
            NewSystem.Version = newVersion;
            _MilitaryManagerData.WeaponSystems.Add(maxKey, NewSystem);
        }

        public void NewReliabilitySystem(int authority, string systemName, double initCost, double cost, int load, int militaryGeneration, bool investigated, bool active, int upgradeCount, List<int> masterClasses, int maneuver, int engine)
        {
            int maxKey = _MilitaryManagerData.ReliabilitySystems.Count == 0? 0: _MilitaryManagerData.ReliabilitySystems.Keys.Max() + 1;
            int newVersion = _MilitaryManagerData.ReliabilitySystems.Where(d => d.Value.SystemName == systemName).Max((d) => d.Value.Version) + 1;
            _MilitaryManagerData.ReliabilitySystems.Add(maxKey, new SystemReliability(authority, systemName, newVersion, initCost, cost, load, militaryGeneration, investigated, active, upgradeCount, masterClasses, maneuver, engine));
        }

        public void NewReliabilitySystem(SystemReliability NewSystem)
        {
            int maxKey = _MilitaryManagerData.ReliabilitySystems.Count == 0? 0: _MilitaryManagerData.ReliabilitySystems.Keys.Max() + 1;
            int newVersion = _MilitaryManagerData.ReliabilitySystems.Where(d => d.Value.SystemName == NewSystem.SystemName).Max((d) => d.Value.Version) + 1;
            NewSystem.Version = newVersion;
            _MilitaryManagerData.ReliabilitySystems.Add(maxKey, NewSystem);
        }

        public void NewElectronicsSystem(int authority, string systemName, double initCost, double cost, int load, int militaryGeneration, bool investigated, bool active, int upgradeCount, List<int> masterClasses, int countermeasures, int radar)
        {
            int maxKey = _MilitaryManagerData.ElectronicsSystems.Count == 0? 0: _MilitaryManagerData.ElectronicsSystems.Keys.Max() + 1;
            int newVersion = _MilitaryManagerData.ElectronicsSystems.Where(d => d.Value.SystemName == systemName).Max((d) => d.Value.Version) + 1;
            _MilitaryManagerData.ElectronicsSystems.Add(maxKey, new SystemElectronics(authority, systemName, newVersion, initCost, cost, load, militaryGeneration, investigated, active, upgradeCount, masterClasses, countermeasures, radar));
        }

        public void NewElectronicsSystem(SystemElectronics NewSystem)
        {
            int maxKey = _MilitaryManagerData.ElectronicsSystems.Count == 0? 0: _MilitaryManagerData.ElectronicsSystems.Keys.Max() + 1;
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

        /// <summary>
        /// Перемещение военных юнитов между пулами
        /// </summary>
        /// <param name="Authority"></param>
        /// <param name="FromType"></param>
        /// <param name="FromID"></param>
        /// <param name="DestType"></param>
        /// <param name="DestID"></param>
        /// <param name="UnitID"></param>
        /// <param name="Amount"></param>
        /// <returns></returns>
        public bool SendMilitaryUnits(int Authority, DestinationTypes FromType, int FromID, DestinationTypes DestType, int DestID, int UnitID, int Amount)
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
                _MilitaryManagerData.MilBases.Add(RegID, new MilitaryBase(RegID, AuthID, ModEditor.ModProperties.Instance.DefaultMilBaseCapacity));
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
            GetMainMilPool(e.RegID).AddUnits(e.UnitID, e.Amount);
        }
        #endregion
    }

    public class MilitaryManager_Ds:ISavable
    {
        public Dictionary<int, MilitaryBase> MilBases;
        public Dictionary<int, IMilitaryUnit> MilitaryUnits; //Список всех существующих юнитов в игре
        public Dictionary<int, SystemBody> BodySystems; //Список всех существующих SystemBody в игре
        public Dictionary<int, SystemWeapon> WeaponSystems; //Список всех существующих SystemWeapon в игре
        public Dictionary<int, SystemReliability> ReliabilitySystems; //Список всех существующих SystemReliability в игре
        public Dictionary<int, SystemElectronics> ElectronicsSystems; //Список всех существующих SystemElectronics в игре
        public List<UnitOnTheWay> MilitaryUnitsOnTheWay; //Юниты в процессе перемещения
        public Dictionary<int, MilitaryPool> MainPools;    //Домашние пулы (индекс - индекс региона)
        public List<SeaPool> _SeaPools;

        public MilitaryManager_Ds()
        {
            MilitaryUnits = new Dictionary<int, IMilitaryUnit>();
            MilBases = new Dictionary<int, MilitaryBase>();
            _SeaPools = new List<SeaPool>();
        }

    }
}
