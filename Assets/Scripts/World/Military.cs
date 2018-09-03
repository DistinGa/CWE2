using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace World
{
    public class MilitaryUnit
    {
        public int Authority;
        public int UnitType;    //Land / Sea / Air
        public int UnitClass;   //Helicopter / Tank / Submarine ...
        public string UnitName;
        public int Body, Weapon1, Weapon2, Reliability, Electronics;    //Установленные системы

        public MilitaryUnit(int authority, int unitType, int unitClass, string unitName, int body, int weapon1, int weapon2, int reliability, int electronics)
        {
            Authority = authority;
            UnitType = unitType;
            UnitClass = unitClass;
            UnitName = unitName;
            Body = body;
            Weapon1 = weapon1;
            Weapon2 = weapon2;
            Reliability = reliability;
            Electronics = electronics;
        }

        public double Cost
        {
            get
            {
                double res = 0;
                res = World.TheWorld.GetSystemBody(Body).Cost
                    + World.TheWorld.GetSystemWeapon(Weapon1).Cost
                    + World.TheWorld.GetSystemWeapon(Weapon2).Cost
                    + World.TheWorld.GetSystemReliability(Reliability).Cost
                    + World.TheWorld.GetSystemElectronics(Electronics).Cost;

                //Бонусы, скидки...
                return res;
            }
        }
    }

    public class UnitSystemBaseClass
    {
        public int Authority;
        public string SystemName;
        public int Version;
        double _InitCost; //Начальная стоимость
        public double Cost;
        public int Load;
        public int MilitaryGeneration;  //С которой система становится доступной для изучения.
        public bool Investigated;   //Система изучена
        public bool Active;         //false - система находится в архиве.
        public int UpgradeCount;    //Количество произведённых апгрейдов (исключая бонусные). Нужно для определения цены апгрейда.
        public List<int> MasterClasses;     //Классы юнитов, на которые можно устанавливать данную систему

        public UnitSystemBaseClass(int authority, string systemName, int version, double initCost, double cost, int load, int militaryGeneration, bool investigated, bool active, int upgradeCount, List<int> masterClasses)
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
        }

        public double InitCost
        {
            get
            {
                return _InitCost;
            }
        }
    }

    public class SystemBody : UnitSystemBaseClass
    {
        public int Armor, Stealth, Capacity;

        public SystemBody(int authority, string systemName, int version, double initCost, double cost, int capacity, int militaryGeneration, bool investigated, bool active, int upgradeCount, List<int> masterClasses,
            int armor, int stealth)
            : base(authority, systemName, version, initCost, cost, 0, militaryGeneration, investigated, active, upgradeCount, masterClasses)
        {
            Armor = armor;
            Stealth = stealth;
            Capacity = capacity;
        }

        public SystemBody Upgrade(int upgradeType)
        {
            SystemBody res = new SystemBody(Authority, SystemName, Version, InitCost, Cost, Capacity, MilitaryGeneration, true, true, UpgradeCount + 1, MasterClasses, Armor, Stealth);

            ModEditor.ModProperties modProperties = ModEditor.ModProperties.Instance;

            if (upgradeType == 4)
                Cost -= Cost * modProperties.MilitarySystemCostDecreaseByUpgrade;
            else
                Cost += InitCost * modProperties.MilitarySystemCostIncreasePerUpgrade;

            switch (upgradeType)
            {
                case 1: //Увеличение первого параметра
                    Armor += modProperties.MilitarySystemParamIncreaseByUpgrade;
                    break;
                case 2: //Увеличение второго параметра
                    Stealth += modProperties.MilitarySystemParamIncreaseByUpgrade;
                    break;
                case 3: //Апгрейд вместимости
                    Capacity *= modProperties.MilitarySystemCapacityUpgrade;
                    break;
                case 4: //Снижение стоимости производства
                    break;
                default:
                    break;
            }

            return res;
        }
    }

    public class SystemWeapon : UnitSystemBaseClass
    {
        public int Hitpoint, Range;
        public List<int> TargetClasses;

        public SystemWeapon(int authority, string systemName, int version, double initCost, double cost, int load, int militaryGeneration, bool investigated, bool active, int upgradeCount, List<int> masterClasses,
        int hitpoint, int range, List<int> targetClasses)
        : base(authority, systemName, version, initCost, cost, load, militaryGeneration, investigated, active, upgradeCount, masterClasses)
        {
            Hitpoint = hitpoint;
            Range = range;
            TargetClasses = targetClasses;
        }

        public SystemWeapon Upgrade(int upgradeType)
        {
            SystemWeapon res = new SystemWeapon(Authority, SystemName, Version, InitCost, Cost, Load, MilitaryGeneration, true, true, UpgradeCount + 1, MasterClasses, Hitpoint, Range, TargetClasses);

            ModEditor.ModProperties modProperties = ModEditor.ModProperties.Instance;

            if (upgradeType == 4)
                Cost -= -InitCost * modProperties.MilitarySystemCostDecreaseByUpgrade;
            else
                Cost += +InitCost * modProperties.MilitarySystemCostIncreasePerUpgrade;

            switch (upgradeType)
            {
                case 1: //Увеличение первого параметра
                    Hitpoint += modProperties.MilitarySystemParamIncreaseByUpgrade;
                    break;
                case 2: //Увеличение второго параметра
                    Range += modProperties.MilitarySystemParamIncreaseByUpgrade;
                    break;
                case 3: //Апгрейд вместимости
                    Load -= modProperties.MilitarySystemCapacityUpgrade;
                    break;
                case 4: //Снижение стоимости производства
                    break;
                default:
                    break;
            }

            return res;
        }
    }

    public class SystemReliability : UnitSystemBaseClass
    {
        public int Maneuver, Engine;

        public SystemReliability(int authority, string systemName, int version, double initCost, double cost, int load, int militaryGeneration, bool investigated, bool active, int upgradeCount, List<int> masterClasses,
            int maneuver, int engine)
            : base(authority, systemName, version, initCost, cost, load, militaryGeneration, investigated, active, upgradeCount, masterClasses)
        {
            Maneuver = maneuver;
            Engine = engine;
        }

        public SystemReliability Upgrade(int upgradeType)
        {
            SystemReliability res = new SystemReliability(Authority, SystemName, Version, InitCost, Cost, Load, MilitaryGeneration, true, true, UpgradeCount + 1, MasterClasses, Maneuver, Engine);

            ModEditor.ModProperties modProperties = ModEditor.ModProperties.Instance;

            if (upgradeType == 4)
                Cost -= -InitCost * modProperties.MilitarySystemCostDecreaseByUpgrade;
            else
                Cost += +InitCost * modProperties.MilitarySystemCostIncreasePerUpgrade;

            switch (upgradeType)
            {
                case 1: //Увеличение первого параметра
                    Maneuver += modProperties.MilitarySystemParamIncreaseByUpgrade;
                    break;
                case 2: //Увеличение второго параметра
                    Engine += modProperties.MilitarySystemParamIncreaseByUpgrade;
                    break;
                case 3: //Апгрейд вместимости
                    Load -= modProperties.MilitarySystemCapacityUpgrade;
                    break;
                case 4: //Снижение стоимости производства
                    break;
                default:
                    break;
            }

            return res;
        }
    }

    public class SystemElectronics : UnitSystemBaseClass
    {
        public int Countermeasures, Radar;

        public SystemElectronics(int authority, string systemName, int version, double initCost, double cost, int load, int militaryGeneration, bool investigated, bool active, int upgradeCount, List<int> masterClasses,
            int countermeasures, int radar)
            : base(authority, systemName, version, initCost, cost, load, militaryGeneration, investigated, active, upgradeCount, masterClasses)
        {
            Countermeasures = countermeasures;
            Radar = radar;
        }

        public SystemElectronics Upgrade(int upgradeType)
        {
            SystemElectronics res = new SystemElectronics(Authority, SystemName, Version, InitCost, Cost, Load, MilitaryGeneration, true, true, UpgradeCount + 1, MasterClasses, Countermeasures, Radar);

            ModEditor.ModProperties modProperties = ModEditor.ModProperties.Instance;

            if (upgradeType == 4)
                Cost -= -InitCost * modProperties.MilitarySystemCostDecreaseByUpgrade;
            else
                Cost += +InitCost * modProperties.MilitarySystemCostIncreasePerUpgrade;

            switch (upgradeType)
            {
                case 1: //Увеличение первого параметра
                    Countermeasures += modProperties.MilitarySystemParamIncreaseByUpgrade;
                    break;
                case 2: //Увеличение второго параметра
                    Radar += modProperties.MilitarySystemParamIncreaseByUpgrade;
                    break;
                case 3: //Апгрейд вместимости
                    Load -= modProperties.MilitarySystemCapacityUpgrade;
                    break;
                case 4: //Снижение стоимости производства
                    break;
                default:
                    break;
            }

            return res;
        }
    }
}
