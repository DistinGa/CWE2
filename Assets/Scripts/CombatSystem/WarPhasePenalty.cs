using System;
using System.Collections.Generic;
using nsMilitary;

namespace nsCombat
{
    /// <summary>
    /// Бонусы и пенальти к характеристикам боевых групп указанных классов юнитов в военной фазе.
    /// </summary>
    public class WarPhasePenalty : IMilitaryUnit
    {
        public int _armor;
        public int _countermeasures;
        public int _engine;
        public int _maneuver;
        public int _radar;
        public int _stealth;
        public int _hitPoints;
        public int _range;

        public List<int> ClassIDs;  // Список классов, для которых действуют данные пенальти/бонусы.
        public int Area;    // 0 - для всех, 1 - для атакующих, -1 - для защищающихся

        public string UnitName
        {
            get
            {
                return "";
            }
        }

        public int Authority
        {
            get { return -1; }
            set {}
        }

        public UnitType UnitType => UnitType.Air;

        public int UnitClass => -1;

        public int Armor => _armor;

        public int Capacity => 0;

        public double Cost => 0;

        public int Countermeasures => _countermeasures;

        public int Engine => _engine;

        public int Maneuver => _maneuver;

        public int Radar => _radar;

        public int StartPosition => -1;

        public int Stealth => _stealth;

        public int Supply => 0;

        public List<int> AvailableWeapons
        {
            get
            {
                return null;
            }
        }

        public int GetFireCost(int weaponID)
        {
            return 0;
        }

        public int GetHitPoints(int weaponID)
        {
            return _hitPoints;
        }

        public int GetRange(int weaponID)
        {
            return _range;
        }

        public List<int> TargetClasses(int weaponID)
        {
            return null;
        }

        public void AddPenalties(WarPhasePenalty penalties)
        {
            _armor += penalties._armor;
            _countermeasures += penalties._countermeasures;
            _engine += penalties._engine;
            _maneuver += penalties._maneuver;
            _radar += penalties._radar;
            _stealth += penalties._stealth;
            _hitPoints += penalties._hitPoints;
            _range += penalties._range;
    }
}
}
