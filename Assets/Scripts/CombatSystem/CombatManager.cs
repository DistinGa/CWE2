using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using nsEventSystem;
using nsMilitary;

namespace nsCombat
{
    public class CombatManager
    {
        public static CombatManager Instance;
        CombatManager_DS _combatManager_Data;

        static CombatManager()
        {
            new CombatManager();
        }

        private CombatManager()
        {
            Instance = this;

            _combatManager_Data = new CombatManager_DS();

            GameEventSystem.Subscribe(GameEventSystem.MyEventsTypes.TurnActions, OnTurn);
            GameEventSystem.Subscribe(GameEventSystem.MyEventsTypes.EndOfCombat, EndOfCombat);
        }

        ~CombatManager()
        {
            GameEventSystem.UnSubscribe(GameEventSystem.MyEventsTypes.TurnActions, OnTurn);
            GameEventSystem.UnSubscribe(GameEventSystem.MyEventsTypes.EndOfCombat, EndOfCombat);
        }

        public CombatManager_DS CombatManager_Data
        {
            get { return _combatManager_Data; }
        }

        public Dictionary<int, CombatData> Combats
        {
            get
            {
                return _combatManager_Data.Combats;
            }
        }

        public List<CombatData> GetCombatsForReg(int RegID)
        {
            List<CombatData> res = new List<CombatData>();

            res = Combats.Values.Where(c => c.RegID == RegID || c.AttackerRegID == RegID).ToList();

            nsWorld.Region_Op region = nsWorld.World.TheWorld.GetRegion(RegID);
            if (region.RegionController != null)
            {
                // Добавляем бои, где юниты отданы в помощь
                res.AddRange(
                    Combats.Values.Where(
                        c => c.AttackerUnits.Values.Where(u => u.Unit.Authority == region.Authority).Count() > 0
                        || c.DefenderUnits.Values.Where(u => u.Unit.Authority == region.Authority).Count() > 0
                        ).ToList()
                    );
            }

            return res;
        }

        public void AddCombat(int RegID, int AttackerRegID)
        {
            var combatData = new CombatData()
            {
                Active = true,
                RegID = RegID,
                AttackerRegID = AttackerRegID,
                CombatArea = GameManager.GM.GameProperties.CombatArea,
                CenterCombatArea = GameManager.GM.GameProperties.CenterCombatArea,
                MovementValue = nsWorld.World.TheWorld.GetRegion(RegID).MovementValue
            };

            //Добавление юнитов

            AddCombat(combatData);
        }

        public void AddCombat(CombatData combatData)
        {
            Combats[combatData.RegID] = combatData;
        }

        public void DeleteCombat(int ID)
        {
            Combats.Remove(ID);
        }



        public void Attack(CombatUnit Attacker, CombatUnit Target, int AttackerWeaponID)
        {
            if (!Attacker.WeaponReady[AttackerWeaponID])
                return;
            if (Attacker.Supply < Attacker.Unit.GetFireCost(AttackerWeaponID))
                return;

            Attacker.Fire(AttackerWeaponID);

            int DamageAmount = Attacker.GetDamageAmount(Target, AttackerWeaponID);

            if (DamageAmount > 0)
            {
                Target.TakeDamage(DamageAmount);
                GameEventSystem.InvokeEvents(GameEventSystem.MyEventsTypes.AttackBattleAction, new AttackBattleAction_EventArgs() { Message = $"{Attacker.Name} -> {Target.Name} -{DamageAmount} HP"});
            }
        }

        public CombatUnit GetCombatUnit(CombatData combatData, bool Attacker, int CombatUnitID)
        {
            Dictionary<int, CombatUnit> CUList;
            if (Attacker)
                CUList = combatData.AttackerUnits;
            else
                CUList = combatData.DefenderUnits;

            return CUList[CombatUnitID];
        }

        public void DeleteCombatUnit(CombatData combatData, bool Attacker, int CombatUnitID)
        {
            if (Attacker)
            {
                if (combatData.AttackerUnits.ContainsKey(CombatUnitID))
                    combatData.AttackerUnits.Remove(CombatUnitID);
            }
            else
            {
                if (combatData.DefenderUnits.ContainsKey(CombatUnitID))
                    combatData.DefenderUnits.Remove(CombatUnitID);
            }
        }

        /// <summary>
        /// Движение к противнику
        /// </summary>
        public void MoveForward(CombatData combatData, CombatUnit CU)
        {
            int _nearestPos = -(combatData.CombatArea - 1); // При пересечении центра первая клетка на вражеской территории имеет индекс 0.

            if (CU.Unit.UnitType == UnitType.Sea && !combatData.SeaAccess
                    || CU.Unit.UnitType == UnitType.Ground && !combatData.GroundAccess
                    || CU.Unit.UnitType == UnitType.Air && !combatData.AirAccess)
                _nearestPos = combatData.CenterCombatArea + 1;

            if (CU.Position > _nearestPos) CU.Move(-1);
        }

        /// <summary>
        /// Движение от противника
        /// </summary>
        public void MoveBackward(CombatData combatData, CombatUnit CU)
        {
            if (CU.Position < combatData.CombatArea) CU.Move(1);
        }

        /// <summary>
        /// Добавление группы юнитов на поле боя (или юнитов в существующую группу)
        /// </summary>
        /// <param name="Attacker">За какую сторону будут воевать юниты (за агрессора или за защищающегося)</param>
        /// <param name="UnitID">MilitaryUnitID в MilitaryManager_Ds.MilitaryUnits</param>
        /// <param name="Amount"></param>
        public void AddCombatUnits(CombatData combatData, bool Attacker, int UnitID, int Amount, int HomeBaseID)
        {
            Dictionary<int, CombatUnit> CUList;
            if (Attacker)
                CUList = combatData.AttackerUnits;
            else
                CUList = combatData.DefenderUnits;

            CombatUnit cu = CUList.Values.First(u => u.UnitID == UnitID &&  u.HomeBaseID == HomeBaseID);
            //Если группа с такими юнитами находится на краю поля боя, добавляем юниты в неё
            if (cu != null && cu.Position == combatData.CombatArea)
                cu.Amount += Amount;
            else
            {
                //Иначе создаём новую
                cu = new CombatUnit(CUList.Keys.Max() + 1, UnitID, Amount, combatData.MovementValue, HomeBaseID, combatData.ReliefPropertiesID, Attacker);
                cu.Position = combatData.CombatArea;
                CUList.Add(cu.ID, cu);
            }
        }

        /// <summary>
        /// Получить список своих юнитов и юнитов противника в указанном бою.
        /// </summary>
        /// <param name="combatData"></param>
        /// <param name="RegID">Индекс региона, для которого юниты - свои</param>
        /// <param name="MyUnits"></param>
        /// <param name="Opponents"></param>
        public void GetUnits(CombatData combatData, int RegID, out List<CombatUnit> MyUnits, out List<CombatUnit> Opponents)
        {
            MyUnits = new List<CombatUnit>();
            Opponents = new List<CombatUnit>();

            if (RegID == combatData.RegID)
            {
                MyUnits = combatData.DefenderUnits.Values.ToList();
                Opponents = combatData.AttackerUnits.Values.ToList();
            }

            if (RegID == combatData.AttackerRegID && combatData.RegID != combatData.AttackerRegID)
            {
                MyUnits = combatData.AttackerUnits.Values.ToList();
                Opponents = combatData.DefenderUnits.Values.ToList();
            }

            //Если юниты отданы в поддержку
            if (RegID != combatData.RegID && RegID != combatData.AttackerRegID)
            {
                nsWorld.Region_Op region = nsWorld.World.TheWorld.GetRegion(RegID);

                var addMyUnits = combatData.AttackerUnits.Values.Where(v => v.Unit.Authority == region.Authority).ToList();
                if (addMyUnits.Count > 0)
                {
                    MyUnits.AddRange(addMyUnits);
                    Opponents = combatData.DefenderUnits.Values.ToList();
                }
                else
                {
                    addMyUnits = combatData.DefenderUnits.Values.Where(v => v.Unit.Authority == region.Authority).ToList();
                    if (addMyUnits.Count > 0)
                    {
                        MyUnits.AddRange(addMyUnits);
                        Opponents = combatData.AttackerUnits.Values.ToList();
                    }
                }
            }
        }

        /// <summary>
        /// Автоматический ход. Неконтролируемые регионы управляют всеми группами юнитов, контролируемые - теми, которым игрок не отдал приказ.
        /// </summary>
        /// <param name="combatData"></param>
        /// <param name="RegID">Регион, который выполняет свой ход</param>
        public void CommonTurn(CombatData combatData, int RegID)
        {
            List<CombatUnit> MyUnits = new List<CombatUnit>();
            List<CombatUnit> Opponents = new List<CombatUnit>();

            if (combatData.RegID == RegID && combatData.AttackerRegID == RegID)
            {
                // Гражданская война. Юниты нападающего и защищающегося принадлежат одному региону.
                // Выполняем автоматический ход сначала для одной стороны, потом для другой.
                MyUnits = combatData.AttackerUnits.Values.ToList();
                Opponents = combatData.DefenderUnits.Values.ToList();
                CommonTurn(combatData, MyUnits, Opponents);

                Opponents = combatData.AttackerUnits.Values.ToList();
                MyUnits = combatData.DefenderUnits.Values.ToList();
                CommonTurn(combatData, MyUnits, Opponents);
            }
            else
            {
                GetUnits(combatData, RegID, out MyUnits, out Opponents);
                // Не понятно, зачем такая проверка. (Если юниты направлены на помощь, такая проверка не пройдёт.)
                //if (combatData.RegID != RegID && combatData.AttackerRegID != RegID)
                //    return;

                CommonTurn(combatData, MyUnits, Opponents);
            }
        }

        private void CommonTurn(CombatData combatData, List<CombatUnit> MyUnits, List<CombatUnit> Opponents)
        {
            foreach (CombatUnit attackerUnit in MyUnits)
            {
                //Берём только группы, которыми не управлял игрок
                if (!attackerUnit.ActionSelected && !attackerUnit.Manual)
                {
                    //Если у группы низкий Armor, убираем её назад
                    if (attackerUnit.RestArmorPercent < 20)
                    {
                        if (attackerUnit.MovementCnt <= 0)
                        {
                            MoveBackward(combatData, attackerUnit);
                            continue;
                        }
                    }

                    //Если Supply меньше, чем FireCost первого оружия, отправляем группу юнитов на перезарядку
                    if (attackerUnit.Supply < attackerUnit.Unit.GetFireCost(attackerUnit.Unit.AvailableWeapons[0]))
                    {
                        attackerUnit.Resupply();
                        continue;
                    }

                    //Атака
                    bool hasTargets = false;
                    foreach (var weaponID in attackerUnit.Unit.AvailableWeapons)
                    {
                        if (attackerUnit.Supply >= attackerUnit.Unit.GetFireCost(weaponID))
                        {
                            List<CombatUnit> targets = attackerUnit.GetTargetsInrange(Opponents, weaponID);
                            //Стрелять будем по самому слабому
                            if (targets != null && targets.Count > 0)
                            {
                                hasTargets = true;
                                CombatUnit target = targets.OrderBy(cu => cu.Armor).First();
                                Attack(attackerUnit, target, weaponID);
                            }
                        }
                    }

                    //Если нет доступных целей, двигаем юнит вперёд
                    if (!hasTargets)
                    {
                        if (attackerUnit.MovementCnt <= 0)
                        {
                            MoveForward(combatData, attackerUnit);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Проверка победы в бою
        /// </summary>
        public void CheckCombatResult(CombatData combatData)
        {
            if (combatData.AttackerMoral <= 0)
            {
                GameEventSystem.InvokeEvents(GameEventSystem.MyEventsTypes.EndOfCombat, new EndOfCombat_EventArgs() { WinnerRegID = combatData.RegID, CombatID = combatData.RegID });
                return;
            }

            if (combatData.DefenderMoral <= 0)
            {
                GameEventSystem.InvokeEvents(GameEventSystem.MyEventsTypes.EndOfCombat, new EndOfCombat_EventArgs() { WinnerRegID = combatData.AttackerRegID, CombatID = combatData.RegID });
                return;
            }

            bool InGreenZone = false;
            foreach (var item in combatData.AttackerUnits.Values)
            {
                if (item.Position <= combatData.CenterCombatArea)
                {
                    InGreenZone = true;
                    break;
                }
            }

            if (!InGreenZone)
            {
                GameEventSystem.InvokeEvents(GameEventSystem.MyEventsTypes.EndOfCombat, new EndOfCombat_EventArgs() { WinnerRegID = combatData.RegID, CombatID = combatData.RegID });
                return;
            }

            InGreenZone = false;
            foreach (var item in combatData.DefenderUnits.Values)
            {
                if (item.Position <= combatData.CenterCombatArea)
                {
                    InGreenZone = true;
                    break;
                }
            }

            if (!InGreenZone)
            {
                GameEventSystem.InvokeEvents(GameEventSystem.MyEventsTypes.EndOfCombat, new EndOfCombat_EventArgs() { WinnerRegID = combatData.AttackerRegID, CombatID = combatData.RegID });
                return;
            }
        }

        /// <summary>
        /// Возвращение войск в пул войны.
        /// </summary>
        void ReturnMilUnits(int regID, bool attacker, List<CombatUnit> CUList)
        {
            foreach (var item in CUList)
            {
                AddUnitsToWar_EventArgs e = new AddUnitsToWar_EventArgs();
                e.WarID = regID;
                e.ForAttacker = attacker;
                e.MilUnitID = item.UnitID;
                e.BaseID = item.HomeBaseID;
                e.Amount = item.Amount;

                GameEventSystem.InvokeEvents(GameEventSystem.MyEventsTypes.AddUnitsToWar, e);
            }
        }

        #region Events
        void OnTurn(object sender, EventArgs e)
        {
            foreach (var combatData in Combats.Values.ToList())
            {
                if (!combatData.Active)
                    return;

                List<int> _tmpList = new List<int>();
                foreach (CombatUnit item in combatData.AttackerUnits.Values.ToList())
                {
                    item.MovementCnt -= item.Engine;
                    item.ActionSelected = false;
                    foreach (var wpID in item.WeaponReady.Keys.ToList())
                    {
                        item.WeaponReady[wpID] = true;
                    }

                    if (item.Armor <= 0)
                        _tmpList.Add(item.ID);
                }

                foreach (var cuID in _tmpList)
                {
                    combatData.AttackerUnits.Remove(cuID);
                }

                _tmpList = new List<int>();
                foreach (CombatUnit item in combatData.DefenderUnits.Values.ToList())
                {
                    item.MovementCnt -= item.Engine;
                    item.ActionSelected = false;
                    foreach (var wpID in item.WeaponReady.Keys.ToList())
                    {
                        item.WeaponReady[wpID] = true;
                    }

                    if (item.Armor <= 0)
                        _tmpList.Add(item.ID);
                }

                foreach (var cuID in _tmpList)
                {
                    combatData.AttackerUnits.Remove(cuID);
                }

                CheckCombatResult(combatData);
            }
        }

        void EndOfCombat(object sender, EventArgs e)
        {
            int WinnerRegID = (e as EndOfCombat_EventArgs).WinnerRegID;
            int combatID = (e as EndOfCombat_EventArgs).CombatID;
            CombatData combatData = Combats[combatID];

            combatData.Active = false;

            //Возвращение войск в пул войны
            ReturnMilUnits(combatData.RegID, true, combatData.AttackerUnits.Values.ToList());
            ReturnMilUnits(combatData.RegID, false, combatData.DefenderUnits.Values.ToList());

            combatData.AttackerUnits.Clear();
            combatData.DefenderUnits.Clear();

            DeleteCombat(combatID);
        }
        #endregion Events
    }

    public class CombatManager_DS:ISavable
    {
        public Dictionary<int, CombatData> Combats; //Все идущие в данный момент бои (Key - индекс региона, где идёт война (Defender)).

        public CombatManager_DS()
        {
            Combats = new Dictionary<int, CombatData>();
        }
    }
}
