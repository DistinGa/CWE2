using System.Collections;
using System.Collections.Generic;

namespace nsMilitary
{
    public class MilitaryUnit : IMilitaryUnit
    {
        public string UnitName { get; private set; }
        public string Version { get; private set; }
        public int Authority { get; set; }
        public UnitType UnitType { get; set; }    //Land / Sea / Air
        public int UnitClass { get; set; }   //Helicopter / Tank / Submarine ...
        public int Body;
        public List<int> Weapon, Reliability, Electronics;    //Установленные системы

        private Dictionary<int, SystemWeapon> _weaponSystems;

        public MilitaryUnit(int authority, UnitType unitType, int unitClass, string unitName, string version)
        {
            Authority = authority;
            UnitType = unitType;
            UnitClass = unitClass;
            UnitName = unitName;
            Version = version;
        }

        public MilitaryUnit(int authority, UnitType unitType, int unitClass, string unitName, string version, int body, List<int> weapon, List<int> reliability, List<int> electronics)
            :this(authority, unitType, unitClass, unitName, version)
        {
            Body = body;
            Weapon = weapon;
            Reliability = reliability;
            Electronics = electronics;
        }

        /// <summary>
        /// Кешированный список оружия.
        /// </summary>
        private Dictionary<int, SystemWeapon> WeaponSystems
        {
            get
            {
                if (_weaponSystems == null)
                {
                    _weaponSystems = new Dictionary<int, SystemWeapon>();
                    foreach (var weaponID in Weapon)
                    {
                        _weaponSystems[weaponID] = MilitaryManager.Instance.GetSystemWeapon(weaponID);
                    }
                }

                return _weaponSystems;
            }
        }

        public List<int> AvailableWeapons
        {
            get { return Weapon; }
        }

        /// <summary>
        /// Начальное положение на поле боя (клетка, начиная от центра)
        /// </summary>
        public int StartPosition
        {
            get { return MilitaryManager.Instance.UnitClasses[UnitClass].StartPosition; }
        }

        public double Cost
        {
            get
            {
                double res = 0;
                res = MilitaryManager.Instance.GetSystemBody(Body).Cost;

                foreach (var item in Weapon)
                {
                    res += MilitaryManager.Instance.GetSystemWeapon(item).Cost;
                }

                foreach (var item in Reliability)
                {
                    res += MilitaryManager.Instance.GetSystemReliability(item).Cost;
                }

                foreach (var item in Electronics)
                {
                    res += MilitaryManager.Instance.GetSystemElectronics(item).Cost;
                }

                //Бонусы, скидки...
                return res;
            }
        }

        public int Armor
        {
            get { return MilitaryManager.Instance.GetSystemBody(Body).Armor; }
        }

        public int Stealth
        {
            get { return MilitaryManager.Instance.GetSystemBody(Body).Stealth; }
        }

        public int Capacity
        {
            get { return MilitaryManager.Instance.GetSystemBody(Body).Capacity; }
        }

        public int Supply
        {
            get
            {
                int res = Capacity;

                foreach (var item in Weapon)
                {
                    res -= MilitaryManager.Instance.GetSystemWeapon(item).Load;
                }
                foreach (var item in Reliability)
                {
                    res -= MilitaryManager.Instance.GetSystemReliability(item).Load;
                }
                foreach (var item in Electronics)
                {
                    res -= MilitaryManager.Instance.GetSystemElectronics(item).Load;
                }

                return res;
            }
        }

        public int Maneuver
        {
            get
            {
                int res = 0;
                foreach (var item in Reliability)
                {
                    res += MilitaryManager.Instance.GetSystemReliability(item).Maneuver;
                }
                return res;
            }
        }

        public int Engine
        {
            get
            {
                int res = 0;
                foreach (var item in Reliability)
                {
                    res += MilitaryManager.Instance.GetSystemReliability(item).Engine;
                }
                return res;
            }
        }

        public int Countermeasures
        {
            get
            {
                int res = 0;
                foreach (var item in Electronics)
                {
                    res += MilitaryManager.Instance.GetSystemElectronics(item).Countermeasures;
                }
                return res;
            }
        }

        public int Radar
        {
            get
            {
                int res = 0;
                foreach (var item in Electronics)
                {
                    res += MilitaryManager.Instance.GetSystemElectronics(item).Radar;
                }
                return res;
            }
        }

        public int Generation
        {
            get
            {
                return MilitaryManager.Instance.GetSystemBody(Body).MilitaryGeneration;
            }
        }

        public int GetFireCost(int weaponID)
        {
            return WeaponSystems[weaponID].FireCost;
        }

        public int GetHitPoints(int weaponID)
        {
            return WeaponSystems[weaponID].Hitpoint;
        }

        public int GetRange(int weaponID)
        {
            return WeaponSystems[weaponID].Range;
        }

        public List<int> TargetClasses(int weaponID)
        {
            return WeaponSystems[weaponID].TargetClasses;
        }

        public IMilitaryUnit Clone()
        {
            return new MilitaryUnit(Authority, UnitType, UnitClass, UnitName, Version, Body, Weapon, Reliability, Electronics);
        }
    }

    public class UnitSystemBaseClass
    {
        private double _InitCost; //Начальная стоимость

        public int Authority;
        public string SystemName;
        public int Version;
        public double Cost;
        public int Load;
        public int MilitaryGeneration;  //С которой система становится доступной для изучения.
        public bool Investigated;   //Система изучена
        public bool Active;         //false - система находится в архиве.
        public int UpgradeCount;    //Количество произведённых апгрейдов (исключая бонусные). Нужно для определения цены апгрейда.
        public List<int> MasterClasses;     //Классы юнитов, на которые можно устанавливать данную систему

        protected int Par1, Par2;  //Параметры системы. В каждом виде системы они имеют своё предназначение.

        public UnitSystemBaseClass(int authority, string systemName, int version, double initCost, double cost, int load, int militaryGeneration, bool investigated, bool active, int upgradeCount, List<int> masterClasses, int par1, int par2)
        {
            Authority = authority;
            SystemName = systemName;
            Version = version;
            _InitCost = initCost;
            Cost = cost;
            Load = load;
            MilitaryGeneration = militaryGeneration;
            Investigated = investigated;
            Active = active;
            UpgradeCount = 0;
            MasterClasses = masterClasses;
            Par1 = par1;
            Par2 = par2;
        }

        public double InitCost
        {
            get
            {
                return _InitCost;
            }
        }

        protected UnitSystemBaseClass BaseUpgrade(int upgradeType)
        {
            UnitSystemBaseClass res = new UnitSystemBaseClass(Authority, SystemName, Version, InitCost, Cost, Load, MilitaryGeneration, true, true, UpgradeCount + 1, MasterClasses, Par1, Par2);

            ModEditor.ModProperties modProperties = ModEditor.ModProperties.Instance;

            if (upgradeType == 4)   //Снижение стоимости производства
                Cost -= -InitCost * modProperties.MilitarySystemCostDecreaseByUpgrade;
            else
                Cost += +InitCost * modProperties.MilitarySystemCostIncreasePerUpgrade;

            switch (upgradeType)
            {
                case 1: //Увеличение первого параметра
                    Par1 += modProperties.MilitarySystemParamIncreaseByUpgrade;
                    break;
                case 2: //Увеличение второго параметра
                    Par2 += modProperties.MilitarySystemParamIncreaseByUpgrade;
                    break;
                case 3: //Апгрейд вместимости/нагрузки
                    Load += modProperties.MilitarySystemCapacityUpgrade;
                    break;
                default:
                    break;
            }

            return res;
        }

    }

    public class SystemBody : UnitSystemBaseClass
    {
        public SystemBody(int authority, string systemName, int version, double initCost, double cost, int capacity, int militaryGeneration, bool investigated, bool active, int upgradeCount, List<int> masterClasses,
            int armor, int stealth)
            : base(authority, systemName, version, initCost, cost, 0, militaryGeneration, investigated, active, upgradeCount, masterClasses, armor, stealth)
        {
        }

        public SystemBody Upgrade(int upgradeType)
        {
            return BaseUpgrade(upgradeType) as SystemBody;
        }

        public int Armor
        {
            get { return Par1; }
        }

        public int Stealth
        {
            get { return Par2; }
        }

        public int Capacity
        {
            get { return Load; }
        }
    }

    public class SystemWeapon : UnitSystemBaseClass
    {
        public List<int> TargetClasses { get; private set; } //Классы юнитов-целей
        public int FireCost;    //Сколько Supply отнимается при выстреле

        public SystemWeapon(int authority, string systemName, int version, double initCost, double cost, int load, int militaryGeneration, bool investigated, bool active, int upgradeCount, List<int> masterClasses,
        int hitpoint, int range, List<int> targetClasses, int fireCost)
        : base(authority, systemName, version, initCost, cost, load, militaryGeneration, investigated, active, upgradeCount, masterClasses, hitpoint, range)
        {
            TargetClasses = targetClasses;
            FireCost = fireCost;
        }


        public SystemWeapon Upgrade(int upgradeType)
        {
            SystemWeapon res;
            res = BaseUpgrade(upgradeType) as SystemWeapon;
            res.TargetClasses = TargetClasses;
            return res;
        }

        public int Hitpoint
        {
            get { return Par1; }
        }

        public int Range
        {
            get { return Par2; }
        }

    }

    public class SystemReliability : UnitSystemBaseClass
    {
        public SystemReliability(int authority, string systemName, int version, double initCost, double cost, int load, int militaryGeneration, bool investigated, bool active, int upgradeCount, List<int> masterClasses,
            int maneuver, int engine)
            : base(authority, systemName, version, initCost, cost, load, militaryGeneration, investigated, active, upgradeCount, masterClasses, maneuver, engine)
        {
        }

        public SystemReliability Upgrade(int upgradeType)
        {
            return BaseUpgrade(upgradeType) as SystemReliability;
        }

        public int Maneuver
        {
            get { return Par1; }
        }

        public int Engine
        {
            get { return Par2; }
        }
    }

    public class SystemElectronics : UnitSystemBaseClass
    {
        public SystemElectronics(int authority, string systemName, int version, double initCost, double cost, int load, int militaryGeneration, bool investigated, bool active, int upgradeCount, List<int> masterClasses,
            int countermeasures, int radar)
            : base(authority, systemName, version, initCost, cost, load, militaryGeneration, investigated, active, upgradeCount, masterClasses, countermeasures, radar)
        {
        }

        public SystemElectronics Upgrade(int upgradeType)
        {
            return BaseUpgrade(upgradeType) as SystemElectronics;
        }

        public int Countermeasures
        {
            get { return Par1; }
        }

        public int Radar
        {
            get { return Par2; }
        }

    }

    [System.Serializable]
    public class UnitClass
    {
        public string NameID;
        public int ClassID;
        public int StartPosition;   //Начальное положение на поле боя (клетка, начиная от центра)

        public string Name
        {
            get { return Assets.SimpleLocalization.LocalizationManager.Localize(NameID); }
        }
    }

    public enum UnitType
    {
        Sea,
        Ground,
        Air
    }
}
