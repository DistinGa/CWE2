﻿using System;
using System.Collections.Generic;
using Assets.SimpleLocalization;

namespace nsMilitary
{
    [Serializable]
    public class NeutralMilitaryUnit : IMilitaryUnit
    {
        int _authority;
        public int _count;
        public string _name;
        public int _generation;
        UnitType _unitType;
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
                return LocalizationManager.Localize(_name);
            }
        }

        public int Authority
        {
            get { return _authority; }
            set { _authority = value; }
        }

        public UnitType UnitType => _unitType;

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

        public int Generation
        {
            get
            {
                return _generation;
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

        public IMilitaryUnit Clone()
        {
            NeutralMilitaryUnit newUnit = new NeutralMilitaryUnit();

            newUnit._authority = _authority;
            newUnit._count = _count;
            newUnit._name = _name;
            newUnit._generation = _generation;
            newUnit._unitType = _unitType;
            newUnit._unitClass = _unitClass;
            newUnit._armor = _armor;
            newUnit._capacity = _capacity;
            newUnit._cost = _cost;
            newUnit._countermeasures = _countermeasures;
            newUnit._engine = _engine;
            newUnit._maneuver = _maneuver;
            newUnit._radar = _radar;
            newUnit._stealth = _stealth;
            newUnit._supply = _supply;
            newUnit._fireCost = _fireCost;
            newUnit._hitPoints = _hitPoints;
            newUnit._range = _range;
            newUnit._targetClasses = _targetClasses;

            return newUnit;
        }
    }
}
