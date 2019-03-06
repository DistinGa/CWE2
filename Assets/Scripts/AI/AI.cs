using System;
using System.Collections.Generic;
using System.Linq;
using nsCombat;
using nsWorld;
using nsMilitary;

namespace nsAI
{
    public class AI
    {
        public void Turn(int HomelandID)
        {
            // Действия в бою
            List<CombatData> combats = CombatManager.Instance.GetCombatsForReg(HomelandID);
            foreach (var item in combats)
            {
                CombatProcessing(item, HomelandID);
                CombatManager.Instance.CommonTurn(item, HomelandID);
            }
        }

        public void CombatProcessing(CombatData combat, int HomelandID)
        {
            var MyUnits = new List<CombatUnit>();
            var Opponents = new List<CombatUnit>();
            List<AIUnitAction> ActionList;

            if (combat.AttackerRegID == HomelandID && combat.RegID == HomelandID)
            {
                // Гражданская война на родине.
                MyUnits = combat.AttackerUnits.Values.ToList();
                Opponents = combat.DefenderUnits.Values.ToList();
                // Атакующие действия.
                ActionList = TargetsAssignings(MyUnits, Opponents);
                foreach (var act in ActionList)
                {
                    CombatManager.Instance.Attack(act.Attacker, act.Target, act.AttackWeaponID);
                }

                Opponents = combat.AttackerUnits.Values.ToList();
                MyUnits = combat.DefenderUnits.Values.ToList();
                // Атакующие действия.
                ActionList = TargetsAssignings(MyUnits, Opponents);
                foreach (var act in ActionList)
                {
                    CombatManager.Instance.Attack(act.Attacker, act.Target, act.AttackWeaponID);
                }
            }
            else
            {
                CombatManager.Instance.GetUnits(combat, HomelandID, out MyUnits, out Opponents);

                // Атакующие действия.
                ActionList = TargetsAssignings(MyUnits, Opponents);
                foreach (var act in ActionList)
                {
                    CombatManager.Instance.Attack(act.Attacker, act.Target, act.AttackWeaponID);
                }
            }

        }

        /// <summary>
        /// Назначение целей.
        /// </summary>
        /// <param name="MyUnits"></param>
        /// <param name="Opponents"></param>
        List<AIUnitAction> TargetsAssignings(List<CombatUnit> MyUnits, List<CombatUnit> Opponents)
        {
            List<AIUnitAction> res = new List<AIUnitAction>();
            int[,] _damageMatrix;

            List<List<int>> _damageList2 = new List<List<int>>();
            List<CombatUnit> _targets = new List<CombatUnit>();
            List<int> _weapons = new List<int>();
            List<int> _weaponsFireCost = new List<int>();
            List<CombatUnit> _combatUnits = new List<CombatUnit>();
            Dictionary<int, List<int>> _dictWeapons = new Dictionary<int, List<int>>();
            Dictionary<int, int> _dictSupply = new Dictionary<int, int>();

            // Боевые группы, которые ещё не делали ход и могут атаковать.
            List<CombatUnit> _attackers = MyUnits.Where(cu => !cu.ActionSelected).ToList();

            foreach (var cu in _attackers)
            {
                // Отбираем нестрелявшее оружие.
                foreach (var weaponID in cu.WeaponReady.Where(w => w.Value).Select(w => w.Key))
                {
                    // Отбираем оружие, для которого хватает боеприпасов.
                    var _fireCost = cu.Unit.GetFireCost(weaponID);
                    if (_fireCost <= cu.Supply)
                    {
                        _weapons.Add(weaponID);
                        _weaponsFireCost.Add(_fireCost);
                        _combatUnits.Add(cu);
                    }

                    // Запоминаем достижимые цели.
                    _targets.AddRange(cu.GetTargetsInrange(Opponents, weaponID));
                }
            }

            // Группировка целей.
            _targets = _targets.GroupBy(cu => cu).Select(g => g.Key).ToList();

            // Заполняем матрицу урона.
            for (int i = 0; i < _weapons.Count; i++)
            {
                _damageList2.Add(new List<int>());
                for (int j = 0; j < _targets.Count; j++)
                {
                    _damageList2[i].Add(_combatUnits[i].GetDamageAmount(_targets[j], _weapons[i]));
                }
            }

            // Удаляем пустые строки.
            for (int i = _weapons.Count - 1; i >= 0 ; i--)
            {
                bool nullRow = true;
                for (int j = 0; j < _targets.Count; j++)
                {
                    if (_damageList2[i][j] != 0)
                    {
                        nullRow = false;
                        break;
                    }
                }
                if (nullRow)
                {
                    _damageList2.RemoveAt(i);
                    _weapons.RemoveAt(i);
                    _weaponsFireCost.RemoveAt(i);
                    _combatUnits.RemoveAt(i);
                }
            }
            // Удаляем пустые столбцы.
            for (int c = _targets.Count - 1; c >= 0; c--)
            {
                bool nullCol = true;
                for (int r = 0; r < _weapons.Count; r++)
                {
                    if (_damageList2[r][c] != 0)
                    {
                        nullCol = false;
                        break;
                    }
                }
                if (nullCol)
                {
                    foreach (var row in _damageList2)
                    {
                        row.RemoveAt(c);
                    }
                    _targets.RemoveAt(c);
                }
            }

            // Нет доступных целей, возвращаем пустой список.
            if (_damageList2.Count == 0)
                return res;

            // Заполнение матрицы урона _damageMatrix на основе списка списков _damageList2.
            _damageMatrix = new int[_damageList2.Count, _damageList2[0].Count];
            for (int r = 0; r < _damageList2.Count; r++)
            {
                for (int c = 0; c < _damageList2[r].Count; c++)
                {
                    _damageMatrix[r, c] = _damageList2[r][c];
                }
            }

            // Заполнение словарей вооружения и боеприпасов.
            for (int i = 0; i < _weapons.Count; i++)
            {
                if (!_dictWeapons.ContainsKey(_combatUnits[i].ID))
                    _dictWeapons.Add(_combatUnits[i].ID, new List<int>());

                _dictWeapons[_combatUnits[i].ID].Add(i);    // нужен индекс в списке
                _dictSupply[_combatUnits[i].ID] = _combatUnits[i].Supply;
            }

            // Расчет оптимального удара.
            GeneticAlgorithm _GA = new GeneticAlgorithm(_damageMatrix, _targets.Select(cu => TargetsValue(cu)).ToArray(), _weaponsFireCost.ToArray(), _dictWeapons, _dictSupply);
            int[] _GAsolution = _GA.GetSolution(1000, true);

            // Формирование результата для вывода.
            for (int i = 0; i < _GAsolution.Length; i++)
            {
                res.Add(new AIUnitAction() { Attacker = _combatUnits[i], Target = _targets[_GAsolution[i]], AttackWeaponID = _weapons[i] });
            }

            return res;
        }

        /// <summary>
        /// Ценность боевой единицы.
        /// </summary>
        /// <param name="combatUnit"></param>
        /// <returns></returns>
        int TargetsValue(CombatUnit combatUnit)
        {
            return combatUnit.Armor;
        }
    }

    struct AIUnitAction
    {
        public CombatUnit Attacker;
        public CombatUnit Target;
        public int AttackWeaponID;

    }
}
