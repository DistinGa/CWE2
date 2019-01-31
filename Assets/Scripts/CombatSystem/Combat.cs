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

        public CombatManager()
        {
            Instance = this;

            GameEventSystem.Subscribe(GameEventSystem.MyEventsTypes.TurnActions, OnTurn);
            GameEventSystem.Subscribe(GameEventSystem.MyEventsTypes.EndOfCombat, EndOfCombat);
        }

        ~CombatManager()
        {
            GameEventSystem.UnSubscribe(GameEventSystem.MyEventsTypes.TurnActions, OnTurn);
            GameEventSystem.UnSubscribe(GameEventSystem.MyEventsTypes.EndOfCombat, EndOfCombat);
        }

        /// <summary>
        /// Получить список юнитов указанного региона
        /// </summary>
        public List<CombatUnit> GetMyUnits(int RegID)
        {
            Combat_DS combatData = nsWorld.World.TheWorld.Combats[RegID];
            List<CombatUnit> res = new List<CombatUnit>();

            if (RegID == combatData.RegID)
                res = combatData.DefenderUnits.Values.ToList();
            else if (RegID == combatData.AttackerRegID)
                res = combatData.AttackerUnits.Values.ToList();

            return res;
        }

        /// <summary>
        /// Получить список юнитов оппонента для указанного региона
        /// </summary>
        /// <param name="RegID"></param>
        /// <returns></returns>
        public List<CombatUnit> GetOpponentsUnits(int RegID)
        {
            Combat_DS combatData = nsWorld.World.TheWorld.Combats[RegID];
            List<CombatUnit> res = new List<CombatUnit>();

            if (RegID == combatData.RegID)
                res = combatData.AttackerUnits.Values.ToList();
            else if (RegID == combatData.AttackerRegID)
                res = combatData.DefenderUnits.Values.ToList();

            return res;
        }

        public void Attack(CombatUnit Attacker, CombatUnit Defender, int AttackerWeaponID)
        {
            if (!Attacker.WeaponReady[AttackerWeaponID])
                return;

            Attacker.Fire(AttackerWeaponID);

            int DamageAmount = Attacker.GetDamageAmount(Defender, AttackerWeaponID);

            if (DamageAmount > 0)
                Defender.TakeDamage(DamageAmount);
        }

        public CombatUnit GetCombatUnit(Combat_DS combatData, bool Attacker, int CombatUnitID)
        {
            Dictionary<int, CombatUnit> CUList;
            if (Attacker)
                CUList = combatData.AttackerUnits;
            else
                CUList = combatData.DefenderUnits;

            return CUList[CombatUnitID];
        }

        /// <summary>
        /// Движение к центру
        /// </summary>
        /// <param name="Attacker"></param>
        /// <param name="CombatUnitID"></param>
        public void MoveForward(Combat_DS combatData, CombatUnit CU)
        {
            if (CU.Position > 1) CU.Move(-1, nsWorld.World.TheWorld.GetRegion(combatData.RegID).MovementValue);
        }

        /// <summary>
        /// Движение от центра
        /// </summary>
        /// <param name="Attacker"></param>
        /// <param name="CombatUnitID"></param>
        public void MoveBackward(Combat_DS combatData, CombatUnit CU)
        {
            if (CU.Position < combatData.CombatArea) CU.Move(1, nsWorld.World.TheWorld.GetRegion(combatData.RegID).MovementValue);
        }

        /// <summary>
        /// Добавление группы юнитов на поле боя (или юнитов в существующую группу)
        /// </summary>
        /// <param name="Attacker">За какую сторону будут воевать юниты (за агрессора или за защищающегося)</param>
        /// <param name="UnitID"></param>
        /// <param name="Amount"></param>
        public void AddCombatUnits(Combat_DS combatData, bool Attacker, int UnitID, int Amount, int HomeBaseID)
        {
            Dictionary<int, CombatUnit> CUList;
            if (Attacker)
                CUList = combatData.AttackerUnits;
            else
                CUList = combatData.DefenderUnits;

            CombatUnit cu = CUList.Values.First(u => u.UnitID == UnitID);
            //Если группа с такими юнитами находится на краю поля боя, добавляем юниты в неё
            if (cu != null && cu.Position == combatData.CombatArea && cu.HomeBaseID == HomeBaseID)
                cu.Amount += Amount;
            else
            {
                //Иначе создаём новую
                cu = new CombatUnit(UnitID, Amount, CUList.Keys.Max(), HomeBaseID);
                cu.Position = combatData.CombatArea;
                CUList.Add(cu.ID, cu);
            }
        }

        /// <summary>
        /// Автоматический ход. Неконтролируемые регионы управляют всеми группами юнитов, контролируемые - теми, которым игрок не отдал приказ.
        /// </summary>
        /// <param name="combatData"></param>
        /// <param name="RegID">Регион, который выполняет свой ход</param>
        public void CommonTurn(Combat_DS combatData, int RegID)
        {
            List<CombatUnit> MyUnits = new List<CombatUnit>();
            List<CombatUnit> Opponents = new List<CombatUnit>();

            if (RegID == combatData.RegID)
            {
                MyUnits = combatData.DefenderUnits.Values.ToList();
                Opponents = combatData.AttackerUnits.Values.ToList();
                CommonTurn_inner(combatData, MyUnits, Opponents);
            }

            if (RegID == combatData.AttackerRegID)
            {
                MyUnits = combatData.AttackerUnits.Values.ToList();
                Opponents = combatData.DefenderUnits.Values.ToList();
                CommonTurn_inner(combatData, MyUnits, Opponents);
            }

            //Если юниты отданы в поддержку
            if (RegID != combatData.RegID && RegID != combatData.AttackerRegID)
            {
                nsWorld.Region_Op region = nsWorld.World.TheWorld.GetRegion(RegID);

                MyUnits = combatData.AttackerUnits.Values.Where(v => v.Unit.Authority == region.Authority).ToList();
                if (MyUnits.Count > 0)
                {
                    Opponents = combatData.DefenderUnits.Values.ToList();
                    CommonTurn_inner(combatData, MyUnits, Opponents);
                }

                MyUnits = combatData.DefenderUnits.Values.Where(v => v.Unit.Authority == region.Authority).ToList();
                if (MyUnits.Count > 0)
                {
                    Opponents = combatData.AttackerUnits.Values.ToList();
                    CommonTurn_inner(combatData, MyUnits, Opponents);
                }

            }
        }

        private void CommonTurn_inner(Combat_DS combatData, List<CombatUnit> MyUnits, List<CombatUnit> Opponents)
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
                            return;
                        }
                    }

                    //Если Supply меньше, чем FireCost первого оружия, отправляем группу юнитов на перезарядку
                    if (attackerUnit.Supply < MilitaryManager.Instance.GetSystemWeapon(attackerUnit.Unit.Weapon[0]).FireCost)
                    {
                        attackerUnit.Resupply();
                        return;
                    }

                    //Атака
                    bool hasTargets = false;
                    foreach (var weaponID in attackerUnit.Unit.Weapon)
                    {
                        if (attackerUnit.Supply >= MilitaryManager.Instance.GetSystemWeapon(weaponID).FireCost)
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
        void CheckCombatResult(Combat_DS combatData)
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
                            MilitaryManager.Instance.SendMilitaryUnits(item.Unit.Authority, DestinationTypes.War, 0, DestinationTypes.MainPool, item.Unit.Authority, item.UnitID, item.Amount);
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
            foreach (var combatData in nsWorld.World.TheWorld.Combats.Values.ToList())
            {
                if (!combatData.Active)
                    return;

                combatData.AttackerMoral -= combatData.AttackerMoralPenalty;
                combatData.DefenderMoral -= combatData.DefenderMoralPenalty;

                foreach (CombatUnit item in combatData.AttackerUnits.Values.ToList())
                {
                    item.MovementCnt--;
                    item.ActionSelected = false;
                }

                foreach (CombatUnit item in combatData.DefenderUnits.Values.ToList())
                {
                    item.MovementCnt--;
                    item.ActionSelected = false;
                }

                CheckCombatResult(combatData);
            }
        }

        void EndOfCombat(object sender, EventArgs e)
        {
            int WinnerRegID = (e as EndOfCombat_EventArgs).WinnerRegID;
            int combatID = (e as EndOfCombat_EventArgs).CombatID;
            Combat_DS combatData = nsWorld.World.TheWorld.Combats[combatID];

            //Возвращение войск "домой"
            ReturnMilUnits(combatData.AttackerUnits.Values.ToList());
            ReturnMilUnits(combatData.DefenderUnits.Values.ToList());

            combatData.AttackerUnits.Clear();
            combatData.DefenderUnits.Clear();

            combatData.Active = false;

            nsWorld.World.TheWorld.DeleteCombat(combatID);
        }
        #endregion Events
    }


    public class Combat_DS
    {
        public bool Active;
        public Dictionary<int, CombatUnit> AttackerUnits, DefenderUnits;
        public int RegID;   //Регион, в котором проходит сражение (защищающийся)
        public int AttackerRegID;   //Регион-агрессор
        public int AttackerMoral, DefenderMoral;
        public int AttackerMoralPenalty, DefenderMoralPenalty;
        public int CombatArea;    //Размер поля боя(количество линий для каждой стороны)
        public int CenterCombatArea;    //Размер центральной облати (количество линий для каждой стороны)
    }
}
