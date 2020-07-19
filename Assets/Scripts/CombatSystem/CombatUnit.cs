using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using nsMilitary;

namespace nsCombat
{
    public class CombatUnit : ISavable
    {
        bool _Manual;    //Группа управляется игроком
        bool _ActionSelected;    //Игрок отдал приказ группе
        int _Armor, _InitArmor;
        int _MovementCnt; //Счётчик перемещения (когда доходит до нуля, юнит может перемещаться)
        int _Position; //Как далеко от центра находится группа (начинается с 1)
        int _Supply;
        int _MovementValue;  // Ограничение на перемещение, накладываемые местностью (начальное значение счётчика, из которого будет вычитаться Engine)

        int _ReliefPropertiesID = -1;
        WarPhasePenalty _ClassPenalty = null;
        bool _IsAttacker;

        int _Amount;
        public int ID { get; private set; }
        public int UnitID { get; private set; }
        public Dictionary<int, bool> WeaponReady { get; private set; }  //Готовность оружия к стрельбе (после выстрела сбрасывается)
        public int HomeBaseID { get; private set; }     //Из какой базы пришли юниты и куда уйдут после окончания боя (-1 - домашний пул)

        public CombatUnit(int ID, int MilitaryUnitID, int Amount, int MovementValue, int HomeBaseID = -1, int ReliefPropertiesID = -1, bool isAttacker = true)
        {
            this.ID = ID;
            UnitID = MilitaryUnitID;
            this.HomeBaseID = HomeBaseID;
            _Amount = Amount;
            _Armor = Amount * Unit.Armor + ClassPenalty.Armor;
            _InitArmor = _Armor;
            _Supply = Unit.Supply;
            _Position = Unit.StartPosition;
            _MovementCnt = 0;
            _ActionSelected = false;
            _MovementValue = MovementValue;
            _ReliefPropertiesID = ReliefPropertiesID;
            _IsAttacker = isAttacker;

            WeaponReady = new Dictionary<int, bool>();
            foreach (var item in Unit.AvailableWeapons)
            {
                WeaponReady.Add(item, true);
            }

        }

        #region Properties
        private WarPhasePenalty ClassPenalty
        {
            get
            {
                if (_ClassPenalty == null)
                {
                    _ClassPenalty = new WarPhasePenalty();

                    if (_ReliefPropertiesID > -1)
                    {
                        if (GameManager.GM.GameProperties.ReliefProperties.ContainsKey(_ReliefPropertiesID))
                            _ClassPenalty = GameManager.GM.GameProperties.ReliefProperties[_ReliefPropertiesID].GetClassPenalties(ClassID, _IsAttacker);
                    }
                }

                return _ClassPenalty;
            }
        }

        public int RestArmorPercent
        {
            get { return (int)(Armor * 100f / _InitArmor); }
        }

        public IMilitaryUnit Unit
        {
            get { return MilitaryManager.Instance.GetMilitaryUnit(UnitID); }
        }

        public bool Manual
        {
            get { return _Manual; }
            set { _Manual = value; }
        }

        public string Name
        {
            get { return Unit.UnitName; }
        }

        public int ClassID
        {
            get { return Unit.UnitClass; }
        }

        public int Armor
        {
            get { return _Armor; }

            set { _Armor = value; }
        }

        public int MovementCnt
        {
            get { return _MovementCnt; }

            set { _MovementCnt = value; }
        }

        /// <summary>
        /// Процент счётчика движения
        /// </summary>
        public float MovementPct
        {
            get { return (float)_MovementCnt / (float)_MovementValue; }
        }

        public int Position
        {
            get { return _Position; }

            set { _Position = value; }
        }

        public int Supply
        {
            get { return _Supply; }

            set { _Supply = value; }
        }

        public int Amount
        {
            get { return _Amount; }

            set { _Amount = value; }
        }

        public int Stealth
        {
            get { return Unit.Stealth + ClassPenalty.Stealth; }
        }

        public int Maneuver
        {
            get { return Unit.Maneuver + ClassPenalty.Maneuver; }
        }

        public int Engine
        {
            get { return Unit.Engine + ClassPenalty.Engine; }
        }

        public int Countermeasures
        {
            get { return Unit.Countermeasures + ClassPenalty.Countermeasures; }
        }

        public int Radar
        {
            get { return Unit.Radar + ClassPenalty.Radar; }
        }

        /// <summary>
        /// Игрок отдал приказ группе
        /// </summary>
        public bool ActionSelected
        {
            get { return _ActionSelected; }
            set { _ActionSelected = value; }
        }
        #endregion

        public List<int> GetTargetClasses(int WeaponID)
        {
            return Unit.TargetClasses(WeaponID);
        }

        public int GetHitpoints(int WeaponID)
        {
            return Unit.GetHitPoints(WeaponID) + ClassPenalty.GetHitPoints(WeaponID);
        }

        public void Fire(int WeaponID)
        {
            WeaponReady[WeaponID] = false;
            Supply -= Unit.GetFireCost(WeaponID);
            _ActionSelected = true;
        }

        /// <summary>
        /// Получение урона и сокращение количества юнитов в группе.
        /// </summary>
        /// <param name="amount"></param>
        public void TakeDamage(int amount)
        {
            Armor -= amount;
            if (Armor % Unit.Armor > 0)
                Amount = Armor / Unit.Armor + 1;
            else
                Amount = Armor / Unit.Armor;
        }

        /// <summary>
        /// Перезарядка
        /// </summary>
        public void Resupply()
        {
            Supply = Unit.Supply;
            _ActionSelected = true;
        }

        /// <summary>
        /// Перемещение боевой группы
        /// </summary>
        /// <param name="CU"></param>
        /// <param name="dir">-1 - к центру; 1 - от центра</param>
        public void Move(int dir)
        {
            if (MovementCnt <= 0)
            {
                dir = Math.Sign(dir);
                Position += dir;
                MovementCnt = _MovementValue;
                _ActionSelected = true;
            }
        }

        /// <summary>
        /// Получить список целей для выбранного оружия
        /// </summary>
        /// <param name="Attacker"></param>
        /// <param name="CombatUnitID"></param>
        /// <param name="WeaponID"></param>
        public List<CombatUnit> GetTargets(List<CombatUnit> Opponents, int WeaponID)
        {
            return Opponents.Where((op) => GetTargetClasses(WeaponID).Contains(op.ClassID)).ToList();
        }

        /// <summary>
        /// Получить список целей в зоне поражения оружия (или в зоне видимости радара, если эта зона меньше)
        /// </summary>
        /// <param name="Attacker"></param>
        /// <param name="CombatUnitID"></param>
        /// <param name="WeaponID"></param>
        /// <returns></returns>
        public List<CombatUnit> GetTargetsInrange(List<CombatUnit> Opponents, int WeaponID)
        {
            return GetTargets(Opponents, WeaponID).Where((target) => InOperationalRange(target, WeaponID)).ToList();
        }

        /// <summary>
        /// Определение урона, который может нанести Attacker по Defender при использовании указанного оружия
        /// </summary>
        /// <param name="AttackerID"></param>
        /// <param name="DefenderID"></param>
        /// <param name="AttackerWeaponID"></param>
        /// <returns></returns>
        public int GetDamageAmount(CombatUnit Defender, int AttackerWeaponID)
        {
            CombatUnit Attacker = this;
            // Сначала проверим операционную доступность цели.
            if (Attacker.GetTargetClasses(AttackerWeaponID).Contains(Defender.ClassID))
            {
                if (!InOperationalRange(Defender, AttackerWeaponID))
                    return 0;
            }
            else
                return 0;

            int DamageAmount = Defender.Maneuver - Attacker.Maneuver;
            if (DamageAmount < 0) DamageAmount = 0;
            DamageAmount = Attacker.GetHitpoints(AttackerWeaponID) - Defender.Countermeasures * DamageAmount;

            return DamageAmount * Attacker.Amount;
        }

        private bool InOperationalRange(CombatUnit target, int WeaponID)
        {
            return Position + target.Position - 1 <= Math.Min(Radar - target.Stealth, Unit.GetRange(WeaponID));
        }
    }
}
