using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using nsEventSystem;
using nsMilitary;

namespace nsCombat
{
    /// <summary>
    /// Класс, представляющий бой.
    /// </summary>
    public class CombatData: ISavable
    {
        public bool Active;
        public Dictionary<int, CombatUnit> AttackerUnits, DefenderUnits;
        public int RegID;   //Регион, в котором проходит сражение (защищающийся)
        public int AttackerRegID;   //Регион-агрессор
        public int AttackerMoral, DefenderMoral;
        public int AttackerMoralPenalty, DefenderMoralPenalty;
        public int CombatArea;    //Размер поля боя(количество линий для каждой стороны)
        public int CenterCombatArea;    //Размер центральной облати (количество линий для каждой стороны)
        public int MovementValue;
        public int ReliefPropertiesID;
        public bool SeaAccess = true, GroundAccess = true, AirAccess = true; //Доступность воздуха, земли, моря в зелёной зоне для боевых действий.
    }
}
