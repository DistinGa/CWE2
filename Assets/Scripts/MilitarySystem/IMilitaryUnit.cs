using System.Collections.Generic;

namespace nsMilitary
{
    public interface IMilitaryUnit
    {
        string UnitName { get; }
        int Authority { get; }
        int UnitType { get; }    //Land / Sea / Air
        int UnitClass { get; }   //Helicopter / Tank / Submarine ...
        int Armor { get; }
        int Capacity { get; }
        double Cost { get; }
        int Countermeasures { get; }
        int Engine { get; }
        int Maneuver { get; }
        int Radar { get; }
        int StartPosition { get; }
        int Stealth { get; }
        int Supply { get; }
        List<int> AvailableWeapons { get; } // Имеющееся оружие
        int GetHitPoints(int weaponID);
        int GetFireCost(int weaponID);
        int GetRange(int weaponID);
        List<int> TargetClasses(int weaponID);
    }
}