using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using nsEventSystem;
using nsMilitary;

namespace Combat
{
    public class Combat
    {
        Combat_DS combatData;
        public int CombatArea, CenterCombatArea;    //Размер поля боя, и центральная облать (количество линий для каждой стороны)

        public Combat()
        {
            GameEventSystem.Subscribe(GameEventSystem.MyEventsTypes.TurnActions, OnTurn);
            GameEventSystem.Subscribe(GameEventSystem.MyEventsTypes.EndOfCombat, EndOfCombat);
        }

        ~Combat()
        {
            GameEventSystem.UnSubscribe(GameEventSystem.MyEventsTypes.TurnActions, OnTurn);
            GameEventSystem.UnSubscribe(GameEventSystem.MyEventsTypes.EndOfCombat, EndOfCombat);
        }

        public Dictionary<int, CombatUnit> AttackerUnits
        {
            get { return combatData.AttackerUnits; }
        }

        public Dictionary<int, CombatUnit> DefenderUnits
        {
            get { return combatData.DefenderUnits; }
        }

        public void Attack(CombatUnit Attacker, CombatUnit Defender, int AttackerWeaponID)
        {
            if (!Attacker.WeaponReady[AttackerWeaponID])
                return;

            Attacker.Fire(AttackerWeaponID);

            int DamageAmount = GetDamageAmount(Attacker, Defender, AttackerWeaponID);

            if (DamageAmount > 0)
                Defender.TakeDamage(DamageAmount);
        }

        /// <summary>
        /// Определение урона, который может нанести Attacker по Defender при использовании указанного оружия
        /// </summary>
        /// <param name="AttackerID"></param>
        /// <param name="DefenderID"></param>
        /// <param name="AttackerWeaponID"></param>
        /// <returns></returns>
        int GetDamageAmount(CombatUnit Attacker, CombatUnit Defender, int AttackerWeaponID)
        {
            int DamageAmount = Defender.Maneuver - Attacker.Maneuver;
            if (DamageAmount < 0) DamageAmount = 0;
            DamageAmount = Attacker.GetHitpoints(AttackerWeaponID) - Defender.Countermeasures * DamageAmount;

            return DamageAmount * Attacker.Amount;
        }

        CombatUnit GetCombatUnit(bool Attacker, int CombatUnitID)
        {
            Dictionary<int, CombatUnit> CUList;
            if (Attacker)
                CUList = AttackerUnits;
            else
                CUList = DefenderUnits;

            return CUList[CombatUnitID];
        }

        /// <summary>
        /// Движение к центру
        /// </summary>
        /// <param name="Attacker"></param>
        /// <param name="CombatUnitID"></param>
        public void MoveForward(CombatUnit CU)
        {
            if (CU.Position > 1) CU.Move(-1, nsWorld.World.TheWorld.GetRegion(combatData.RegID).MovementValue);
        }

        /// <summary>
        /// Движение от центра
        /// </summary>
        /// <param name="Attacker"></param>
        /// <param name="CombatUnitID"></param>
        public void MoveBackward(CombatUnit CU)
        {
            if (CU.Position < CombatArea) CU.Move(1, nsWorld.World.TheWorld.GetRegion(combatData.RegID).MovementValue);
        }

        /// <summary>
        /// Перезарядка
        /// </summary>
        public void Resupply(bool Attacker, int CombatUnitID)
        {
            GetCombatUnit(Attacker, CombatUnitID).Resupply();
        }

        /// <summary>
        /// Получить список целей для выбранного оружия
        /// </summary>
        /// <param name="Attacker"></param>
        /// <param name="CombatUnitID"></param>
        /// <param name="WeaponID"></param>
        public List<CombatUnit> GetTargets(bool Attacker, CombatUnit CombatUnit, int WeaponID)
        {
            //Dictionary<int, CombatUnit> OppList;
            List<CombatUnit> OppList;
            if (!Attacker)
                OppList = AttackerUnits.Values.ToList();
            else
                OppList = DefenderUnits.Values.ToList();

            return OppList.Where((op) => CombatUnit.GetTargetClasses(WeaponID).Contains(op.Class)).ToList();
        }

        /// <summary>
        /// Получить список целей в зоне поражения оружия (или в зоне видимости радара, если эта зона меньше)
        /// </summary>
        /// <param name="Attacker"></param>
        /// <param name="CombatUnitID"></param>
        /// <param name="WeaponID"></param>
        /// <returns></returns>
        public List<CombatUnit> GetTargetsInrange(bool Attacker, CombatUnit CombatUnit, int WeaponID)
        {
            return GetTargets(Attacker, CombatUnit, WeaponID).Where((target) => CombatUnit.Position + target.Position - 1 < Math.Min(CombatUnit.Radar, MilitaryManager.Instance.GetSystemWeapon(WeaponID).Range)).ToList();
        }

        /// <summary>
        /// Добавление группы юнитов на поле боя (или юнитов в существующую группу)
        /// </summary>
        /// <param name="Attacker"></param>
        /// <param name="UnitID"></param>
        /// <param name="Amount"></param>
        public void AddCombatUnits(bool Attacker, int UnitID, int Amount, int HomeBaseID)
        {
            Dictionary<int, CombatUnit> CUList;
            if (Attacker)
                CUList = AttackerUnits;
            else
                CUList = DefenderUnits;

            CombatUnit cu = CUList.Values.First(u => u.UnitID == UnitID);
            //Если группа с такими юнитами находится на краю поля боя, добавляем юниты в неё
            if (cu != null && cu.Position == CombatArea && cu.HomeBaseID == HomeBaseID)
                cu.Amount += Amount;
            else
            {
                //Иначе создаём новую
                cu = new CombatUnit(UnitID, Amount, CUList.Keys.Max(), HomeBaseID);
                cu.Position = CombatArea;
                CUList.Add(cu.ID, cu);
            }
        }

        /// <summary>
        /// Автоматический ход. Неконтролируемые регионы управляют всеми группами юнитов, контролируемые - теми, которым игрок не отдал приказ.
        /// </summary>
        public void CommonTurn(int RegID)
        {
            bool Attacker = (RegID == combatData.AttackerRegID);
            List<CombatUnit> CUList;
            if (Attacker)
                CUList = AttackerUnits.Values.ToList();
            else
                CUList = DefenderUnits.Values.ToList();

            foreach (CombatUnit attackerUnit in CUList)
            {
                //Берём только группы, которыми не управлял игрок
                if (!attackerUnit.ActionSelected && !attackerUnit.Manual)
                {
                    //Если у группы низкий Armor, убираем её назад
                    if (attackerUnit.RestArmorPercent < 20)
                    {
                        if (attackerUnit.MovementCnt <= 0)
                        {
                            MoveBackward(attackerUnit);
                            return;
                        }
                    }

                    //Если Supply меньше, чем FireCost первого оружия, отправляем группу юнитов на перезарядку
                    if (attackerUnit.Supply < MilitaryManager.Instance.GetSystemWeapon(attackerUnit.Unit.Weapon[0]).FireCost)
                    {
                        attackerUnit.Resupply();
                        return;
                    }

                    bool hasTargets = false;
                    foreach (var weaponID in attackerUnit.Unit.Weapon)
                    {
                        if (attackerUnit.Supply >= MilitaryManager.Instance.GetSystemWeapon(weaponID).FireCost)
                        {
                            List<CombatUnit> targets = GetTargetsInrange(Attacker, attackerUnit, weaponID);
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
                            MoveForward(attackerUnit);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Проверка победы в бою
        /// </summary>
        void CheckCombatResult()
        {
            if (combatData.AttackerMoral <= 0)
            {
                GameEventSystem.InvokeEvents(GameEventSystem.MyEventsTypes.EndOfCombat, new EndOfCombat_EventArgs() { WinnerRegID = combatData.RegID});
                return;
            }

            if (combatData.DefenderMoral <= 0)
            {
                GameEventSystem.InvokeEvents(GameEventSystem.MyEventsTypes.EndOfCombat, new EndOfCombat_EventArgs() { WinnerRegID = combatData.AttackerRegID });
                return;
            }

            bool InGreenZone = false;
            foreach (var item in combatData.AttackerUnits.Values)
            {
                if (item.Position <= CenterCombatArea)
                {
                    InGreenZone = true;
                    break;
                }
            }

            if (!InGreenZone)
            {
                GameEventSystem.InvokeEvents(GameEventSystem.MyEventsTypes.EndOfCombat, new EndOfCombat_EventArgs() { WinnerRegID = combatData.RegID });
                return;
            }

            InGreenZone = false;
            foreach (var item in combatData.DefenderUnits.Values)
            {
                if (item.Position <= CenterCombatArea)
                {
                    InGreenZone = true;
                    break;
                }
            }

            if (!InGreenZone)
            {
                GameEventSystem.InvokeEvents(GameEventSystem.MyEventsTypes.EndOfCombat, new EndOfCombat_EventArgs() { WinnerRegID = combatData.AttackerRegID });
                return;
            }
        }

        /// <summary>
        /// Возвращение войск "домой"
        /// </summary>
        /// <param name="CUList"></param>
        void ReturnMilUnits(List<CombatUnit> CUList)
        {
            int BaseID = -1;
            foreach (var item in CUList)
            {
                //Юниты нейтральной страны никуда не возвращаются, т.к. и не перемещаются, а значит они и так дома :)
                if (item.Unit.Authority > -1)
                {
                    if (item.Amount > 0)
                    {
                        if (item.HomeBaseID == -1)
                        {
                            //Домашний пул
                            BaseID = item.Unit.Authority;
                            MilitaryManager.Instance.SendMilitaryUnits(item.Unit.Authority, DestinationTypes.War, 0, DestinationTypes.MainPool, BaseID, item.UnitID, item.Amount);
                        }
                        else
                        {
                            //Военная база
                            BaseID = item.HomeBaseID;
                            MilitaryManager.Instance.SendMilitaryUnits(item.Unit.Authority, DestinationTypes.War, 0, DestinationTypes.MilitaryBase, BaseID, item.UnitID, item.Amount);
                        }
                    }
                }
            }
        }

        #region Events
        void OnTurn(object sender, EventArgs e)
        {
            combatData.AttackerMoral -= combatData.AttackerMoralPenalty;
            combatData.DefenderMoral -= combatData.DefenderMoralPenalty;

            foreach (CombatUnit item in AttackerUnits.Values.ToList())
            {
                item.MovementCnt--;
                item.ActionSelected = false;
            }

            foreach (CombatUnit item in DefenderUnits.Values.ToList())
            {
                item.MovementCnt--;
                item.ActionSelected = false;
            }
        }

        void EndOfCombat(object sender, EventArgs e)
        {
            int WinnerRegID = (e as EndOfCombat_EventArgs).WinnerRegID;

            //Возвращение войск "домой"
            ReturnMilUnits(AttackerUnits.Values.ToList());
            ReturnMilUnits(DefenderUnits.Values.ToList());
        }
        #endregion Events
    }


    public class Combat_DS
    {
        public Dictionary<int, CombatUnit> AttackerUnits, DefenderUnits;
        public int RegID;   //Регион, в котором проходит сражение (защищающийся)
        public int AttackerRegID;   //Регион-агрессор
        public int AttackerMoral, DefenderMoral;
        public int AttackerMoralPenalty, DefenderMoralPenalty;
    }
}
