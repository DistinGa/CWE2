using System;
using System.Collections.Generic;

namespace nsMilitary
{
    [Serializable]
    public class NeutralMilitaryUnit : IMilitaryUnit
    {
        int _authority;
        public int _count;
        public string _name;
        int _unitType;
        public int _unitClass;
        public int _armor;
        int _capacity;
        double _cost;
        public int _countermeasures;
        public int _engine;
        public int _maneuver;
        public int _radar;
        public int _stealth;
        public int _supply;
        //public List<int> _availableWeapons = new List<int>();
        public int _fireCost;
        public int _hitPoints;
        public int _range;
        public List<int> _targetClasses;

        public string UnitName
        {
            get
            {
                return _name;
            }
        }

        public int Authority
        {
            get { return _authority; }
            set { _authority = value; }
        }

        public int UnitType => _unitType;

        public int UnitClass => _unitClass;

        public int Armor => _armor;

        public int Capacity => _capacity;

        public double Cost => _cost;

        public int Countermeasures => _countermeasures;

        public int Engine => _engine;

        public int Maneuver => _maneuver;

        public int Radar => _radar;

        public int StartPosition => MilitaryManager.Instance.UnitClasses[UnitClass].StartPosition;

        public int Stealth => _stealth;

        public int Supply => _supply;

        public List<int> AvailableWeapons
        {
            get
            {
                List<int> _availableWeapons = new List<int>();
                _availableWeapons.Add(0);
                return _availableWeapons;
            }
        }

        public int GetFireCost(int weaponID)
        {
            return _fireCost;
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
            return _targetClasses;
        }
    }
}
