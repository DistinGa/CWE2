using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using nsCombat;
using nsWorld;
using nsMilitary;
using nsEventSystem;

namespace nsCombat
{
    public enum WarPhase
    {
        None,
        SeaBattle,
        CoastBattle,
        BaseBattle,
        AirBattle,
        CapitalBattle
    }

    public class WarManager
    {
        WarManager_DS _warManagerData;

        public WarManager()
        {
            _warManagerData = new WarManager_DS();

            GameEventSystem.Subscribe(GameEventSystem.MyEventsTypes.TurnActions, OnTurn);
            GameEventSystem.Subscribe(GameEventSystem.MyEventsTypes.AddUnitsToWar, OnTurn);
        }

        ~WarManager()
        {
            GameEventSystem.UnSubscribe(GameEventSystem.MyEventsTypes.TurnActions, OnTurn);
            GameEventSystem.UnSubscribe(GameEventSystem.MyEventsTypes.AddUnitsToWar, OnTurn);
        }

        void OnTurn(object sender, EventArgs e)
        {
            Dictionary<int, bool> _warsToFinish = new Dictionary<int, bool>();  // Словарь для сохранения войн, окончившихся по морали.

            foreach (var item in _warManagerData.Wars.Values.ToList())
            {
                // Проверка начала интервенции.
                if (!item.Active && item.AttackerUnits.Count > 0)
                {
                    StartRegularWar(item);
                    continue;
                }

                // Проверка условия для контратаки.
                if (item.CurrentPhase == WarPhase.None)
                {
                    if (++item.NonePhaseTurns >= ModEditor.ModProperties.Instance.NonePhaseTurns)
                    {
                        CounterAttack(item);
                    }
                }

                // Изменение морали.
                item.AttackerMoral -= item.AttackerMoralPenalty;
                item.DefenderMoral -= item.DefenderMoralPenalty;

                // Проверка окончания войны по морали.
                if (item.AttackerMoral <= 0)
                    _warsToFinish[item.RegionID] = true;

                if (item.DefenderMoral <= 0)
                    _warsToFinish[item.RegionID] = false;   // Если мораль закончилась одновременно, считаем, что победило государство.
            }

            // Завершение сохранённых войн.
            foreach (var item in _warsToFinish)
            {
                EndWar(_warManagerData.Wars[item.Key], item.Value);
            }
        }

        /// <summary>
        /// Начало обычной войны. Сначала создаётся объект War со сброшенным флагом Active, после прихода войск война становится активной и начинается первая фаза.
        /// </summary>
        public void CreateRegularWar(int regionID, int initiatorRadicals, int aggressorAuthorityID)
        {
            War newWar = new War();
            newWar.Active = false;
            newWar.RegionID = regionID;
            newWar.AggressorAuthorityID = aggressorAuthorityID;

            newWar.InitiatorRadicals = initiatorRadicals;
            newWar.AttackerMoralPenalty = ModEditor.ModProperties.Instance.AggressorMoralPenalty;
            newWar.DefenderMoralPenalty = 0;
        }

        void StartRegularWar(War war)
        {
            war.DefenderMoral = World.TheWorld.GetRegion(war.RegionID).Moral;
            war.AttackerMoral = World.TheWorld.GetRegion(World.TheWorld.GetRegionController(war.AggressorAuthorityID).HomelandID).Moral;

            MilitaryDistribution(war, MilitaryManager.Instance.GetMainMilPool(war.RegionID));
            war.Active = true;

            // Если в пуле войны есть морские юниты (пришедшие из морского пула), начинаем "вторжение с моря", иначе - с суши.
            if (war.AttackerUnits.Where(b => b.Key == -11).Count() > 0)
            {
                BeginPhase(war, WarPhase.SeaBattle);
            }
            else
            {
                BeginPhase(war, WarPhase.CoastBattle);
            }
        }

        /// <summary>
        /// Начало гражданской войны.
        /// </summary>
        public void StartCivilWar(int regionID, int initiatorRadicals)
        {
            War newWar = new War();

            newWar.RegionID = regionID;
            newWar.AggressorAuthorityID = regionID;

            newWar.InitiatorRadicals = initiatorRadicals;
            newWar.AttackerMoral = int.MaxValue;
            newWar.DefenderMoral = int.MaxValue;
            newWar.AttackerMoralPenalty = 0;
            newWar.DefenderMoralPenalty = 0;

            MilitaryDistribution(newWar, MilitaryManager.Instance.GetMainMilPool(regionID));
            newWar.Active = true;
            BeginPhase(newWar, WarPhase.CapitalBattle);
        }

        /// <summary>
        /// Рвспределение войск между государством и радикалами.
        /// </summary>
        /// <param name="war"></param>
        /// <param name="milPool"></param>
        private void MilitaryDistribution(War war, MilitaryPool milPool)
        {
            var _units = milPool.GetUnits();
            int _unitID;
            int _amount;
            int _tmpAmount;
            float _defenderFP, _attackerFp, _currentUnitFP;
            float part;

            while (_units.Count > 0)
            {
                _unitID = _units.Keys.ToList()[0];  // индекс первого юнита в пуле
                _amount = _units[_unitID];
                _currentUnitFP = MilitaryManager.Instance.GetUnitFirepower(_unitID);
                _defenderFP = WarPoolFirepower(war.DefenderUnits);
                _attackerFp = WarPoolFirepower(war.AttackerUnits);

                if (Math.Abs(_defenderFP - _attackerFp) > float.Epsilon)
                {
                    // Если в пуле войны есть юниты, и firepower защищающегося и нападающего не равны.
                    if (_defenderFP < _attackerFp)
                    {
                        _tmpAmount = (int)((_attackerFp - _defenderFP) / _currentUnitFP);
                        // Если firepower юнитов текущей модели из домашнего пула региона не хватает, чтобы покрыть разницу, отдаём защищающемуся всех.
                        if (_tmpAmount > _amount)
                            _tmpAmount = _amount;

                        if (_tmpAmount > 0)
                        {
                            AddUnits(war, false, _unitID, -1, _tmpAmount);
                            _amount -= _tmpAmount;
                        }
                    }
                    else // _defenderFP > _attackerFp
                    {
                        _tmpAmount = (int)((_defenderFP - _attackerFp) / _currentUnitFP);
                        // Если firepower юнитов текущей модели из домашнего пула региона не хватает, чтобы покрыть разницу, отдаём нападающему всех.
                        if (_tmpAmount > _amount)
                            _tmpAmount = _amount;

                        if (_tmpAmount > 0)
                        {
                            AddUnits(war, true, _unitID, -1, _tmpAmount);
                            _amount -= _tmpAmount;
                        }
                    }
                }

                // Если на предыдущем этапе firepower нападающего и защищающегося были уравновешены, то дальше распределяем поровну (примерно).
                if (_amount > 0)
                {
                    _tmpAmount = _amount / 2;

                    if (_tmpAmount > 0)
                        AddUnits(war, true, _unitID, -1, _tmpAmount);

                    AddUnits(war, false, _unitID, -1, _amount - _tmpAmount);
                }

                // Удаляем добавленные юниты из домашнего пула.
                milPool.AddUnits(_unitID, -_units[_unitID]);
            }
        }

        /// <summary>
        /// Firepower части пула войны (защитников или нападающих)
        /// </summary>
        /// <param name="warPool"></param>
        /// <returns></returns>
        private float WarPoolFirepower(Dictionary<int, Dictionary<int, int>> warPool)
        {
            float _res = 0;

            foreach (var baseID in warPool.Keys)
            {
                foreach (var item in warPool[baseID])
                {
                    _res += MilitaryManager.Instance.GetUnitFirepower(item.Key) * item.Value;
                }
            }

            return _res;
        }

        /// <summary>
        /// Возвращает список доступных фаз для нападающего.
        /// </summary>
        /// <param name="war"></param>
        /// <returns></returns>
        public List<WarPhase> GetAvailableWarPhases(War war)
        {
            List<WarPhase> res = new List<WarPhase>();

            if (war.CompletedWarPhases.Contains(WarPhase.SeaBattle) || war.CompletedWarPhases.Contains(WarPhase.CoastBattle))
            {
                if (!war.CompletedWarPhases.Contains(WarPhase.SeaBattle))
                    res.Add(WarPhase.SeaBattle);

                if (!war.CompletedWarPhases.Contains(WarPhase.CoastBattle))
                    res.Add(WarPhase.CoastBattle);

                if (!war.CompletedWarPhases.Contains(WarPhase.AirBattle))
                    res.Add(WarPhase.AirBattle);

                if (!war.CompletedWarPhases.Contains(WarPhase.BaseBattle))
                    res.Add(WarPhase.BaseBattle);

                res.Add(WarPhase.CapitalBattle);
            }
            else
            {
                // Если никакие фазы ещё не пойдены.
                res.Add(WarPhase.SeaBattle);
                res.Add(WarPhase.CoastBattle);
            }

            return res;
        }

        /// <summary>
        /// Начало фазы войны.
        /// </summary>
        /// <param name="phase"></param>
        void BeginPhase(War war, WarPhase phase)
        {
            if (!GetAvailableWarPhases(war).Contains(phase))
            {
                throw new Exception($"Can't start phase <{phase.ToString()}>");
            }

            // Начать бой.
            CombatData _combat = new CombatData();
            _combat.Active = true;
            _combat.AttackerRegID = war.AggressorAuthorityID;
            _combat.RegID = war.RegionID;
            _combat.AttackerUnits = new Dictionary<int, CombatUnit>();
            _combat.DefenderUnits = new Dictionary<int, CombatUnit>();
            _combat.CombatArea = ModEditor.ModProperties.Instance.CombatArea;
            _combat.CenterCombatArea = ModEditor.ModProperties.Instance.CenterCombatArea;
            _combat.MovementValue = World.TheWorld.GetRegion(war.RegionID).MovementValue;

            _combat.ReliefPropertiesID = -1;
            if (ModEditor.ModProperties.Instance.RegPhaseReliefProperties.ContainsKey(war.RegionID))
            {
                if(ModEditor.ModProperties.Instance.RegPhaseReliefProperties[war.RegionID].ContainsKey(phase))
                    _combat.ReliefPropertiesID = ModEditor.ModProperties.Instance.RegPhaseReliefProperties[war.RegionID][phase];
            }

            switch (phase)
            {
                case WarPhase.SeaBattle:
                    _combat.SeaAccess = true;
                    _combat.AirAccess = false;
                    _combat.GroundAccess = false;
                    break;
                case WarPhase.CoastBattle:
                    _combat.SeaAccess = true;
                    _combat.AirAccess = true;
                    _combat.GroundAccess = true;
                    break;
                case WarPhase.BaseBattle:
                    _combat.SeaAccess = true;
                    _combat.AirAccess = true;
                    _combat.GroundAccess = true;
                    break;
                case WarPhase.AirBattle:
                    _combat.SeaAccess = false;
                    _combat.AirAccess = true;
                    _combat.GroundAccess = false;
                    break;
                case WarPhase.CapitalBattle:
                    _combat.SeaAccess = false;
                    _combat.AirAccess = true;
                    _combat.GroundAccess = true;
                    break;
                default:
                    _combat.SeaAccess = true;
                    _combat.AirAccess = true;
                    _combat.GroundAccess = true;
                    break;
            }

            // Перекинуть войска из пула войны в бой.
            foreach (int baseID in war.DefenderUnits.Keys)
            {
                foreach (var item in war.DefenderUnits[baseID])
                {
                    int _CUID = _combat.DefenderUnits.Count;
                    _combat.DefenderUnits[_CUID] = new CombatUnit(_CUID, item.Key, item.Value, _combat.MovementValue, baseID, _combat.ReliefPropertiesID, false);
                }
            }

            foreach (int baseID in war.AttackerUnits.Keys)
            {
                foreach (var item in war.AttackerUnits[baseID])
                {
                    int _CUID = _combat.AttackerUnits.Count;
                    _combat.AttackerUnits[_CUID] = new CombatUnit(_CUID, item.Key, item.Value, _combat.MovementValue, baseID, _combat.ReliefPropertiesID, true);
                }
            }

            war.DefenderUnits.Clear();
            war.AttackerUnits.Clear();

            CombatManager.Instance.AddCombat(_combat);

            // Установить значения.
            war.CurrentPhase = phase;
            war.NonePhaseTurns = 0;
        }

        /// <summary>
        /// Окончание фазы войны. Раздача пенальти, отметка фазы, как пройденной.
        /// </summary>
        /// <param name="attackerWin"></param>
        void EndPhase(War warData, bool attackerWin)
        {
            if (attackerWin)
            {
                if (warData.CurrentPhase == WarPhase.CapitalBattle)
                {
                    // Конец войны. Победа радикалов
                    EndWar(warData, attackerWin);
                }
                else
                {
                    // Фаза пройдена.
                    warData.CompletedWarPhases.Add(warData.CurrentPhase);
                    warData.DefenderMoralPenalty += ModEditor.ModProperties.Instance.RetreatMoralPenalty;

                    // Если это фаза вторжения с моря, открывается возможность переброски войск через морской пул.
                    if (warData.CurrentPhase == WarPhase.SeaBattle && attackerWin)
                    {
                        MilitaryManager.Instance.GetSeaPool(World.TheWorld.GetRegion(warData.RegionID).SeaPoolID).GetNavy(warData.AggressorAuthorityID).Active = true;
                    }
                }
            }
            else
            {
                warData.AttackerMoralPenalty += ModEditor.ModProperties.Instance.RetreatMoralPenalty;
                // Фаза делается непройденной.
                if (warData.CompletedWarPhases.Contains(warData.CurrentPhase))
                    warData.CompletedWarPhases.Remove(warData.CurrentPhase);

                // Если не осталось пройденных фаз, значит либо не удалось вторжение, либо силы государства смогли вытеснить силы агрессора.
                if(warData.CompletedWarPhases.Count == 0)
                    EndWar(warData, attackerWin);
            }

            warData.CurrentPhase = WarPhase.None;
        }

        /// <summary>
        /// Окончание войны. Возврат войск. Смена режима. Обнуление ProsperityLevel.
        /// </summary>
        /// <param name="attackerWin"></param>
        void EndWar(War war, bool attackerWin)
        {
            var _reg = World.TheWorld.GetRegion(war.RegionID);

            if (attackerWin)
            {
                // Победа радикалов.
                GameEventSystem.InvokeEvents(GameEventSystem.MyEventsTypes.ChangeAuthority, new ChangeAuthority_EventArgs() {RegionID = war.RegionID, NewAuthorityID = war.AggressorAuthorityID });
                //Обнуление уровня благосостояния.
                GameEventSystem.InvokeEvents(GameEventSystem.MyEventsTypes.AddProsperity, new AddIntPropertyInRegion_EventArgs() { RegionID = war.RegionID, Amount = -_reg.ProsperityLevel });
            }
            else
            {
                // Победа государства.
                //Обнуление уровня благосостояния.
                GameEventSystem.InvokeEvents(GameEventSystem.MyEventsTypes.AddProsperity, new AddIntPropertyInRegion_EventArgs() { RegionID = war.RegionID, Amount = -_reg.ProsperityLevel });
            }

            // Возврат войск "домой".
            foreach (var item in war.AttackerUnits)
            {
                ReturnMilUnits(war, item.Value, item.Key);
            }
            foreach (var item in war.DefenderUnits)
            {
                ReturnMilUnits(war, item.Value, item.Key);
            }

            war.AttackerUnits.Clear();
            war.DefenderUnits.Clear();

            _warManagerData.Wars.Remove(war.RegionID);
        }

        /// <summary>
        /// Возвращение войск "домой".
        /// </summary>
        /// <param name="milUnits">Key - MilitaryUnitID; Value - amount</param>
        void ReturnMilUnits(War war, Dictionary<int, int> milUnits, int BaseID)
        {
            foreach (var item in milUnits)
            {
                if (item.Value > 0)
                {
                    var _authorityOfMilUnit = MilitaryManager.Instance.GetMilitaryUnit(item.Key).Authority;

                    if (BaseID == -1)
                    {
                        //Домашний пул
                        BaseID = World.TheWorld.GetRegionController(_authorityOfMilUnit).HomelandID;
                        MilitaryManager.Instance.SendMilitaryUnits(_authorityOfMilUnit, DestinationTypes.War, 0, DestinationTypes.MainPool, BaseID, item.Key, item.Value);
                    }
                    else if (BaseID == -11)
                    {
                        //Морской пул.
                        BaseID = World.TheWorld.GetRegion(war.RegionID).SeaPoolID;
                        MilitaryManager.Instance.SendMilitaryUnits(_authorityOfMilUnit, DestinationTypes.War, 0, DestinationTypes.SeaPool, BaseID, item.Key, item.Value);
                    }
                    else
                    {
                        //Военная база
                        MilitaryManager.Instance.SendMilitaryUnits(_authorityOfMilUnit, DestinationTypes.War, 0, DestinationTypes.MilitaryBase, BaseID, item.Key, item.Value);
                    }
                }
            }
        }

        void Retreat(War war)
        {
            // Сдаться может только агрессор.
            EndWar(war, false);
        }

        void AddUnits(object sender, EventArgs e)
        {
            AddUnitsToWar_EventArgs args = e as AddUnitsToWar_EventArgs;
            AddUnits(_warManagerData.Wars[args.WarID], args.ForAttacker, args.MilUnitID, args.BaseID, args.Amount);
        }

        /// <summary>
        /// Добавить войска в войну.
        /// </summary>
        /// <param name="war"></param>
        /// <param name="forAttacker">Для агрессора</param>
        /// <param name="milUnitID"></param>
        /// <param name="baseID">-1 - из домашнего пула</param>
        /// <param name="amount"></param>
        void AddUnits(War war, bool forAttacker, int milUnitID, int baseID, int amount)
        {
            int regID = forAttacker ? war.AggressorAuthorityID : war.RegionID;

            if (CombatManager.Instance.Combats.ContainsKey(regID) && CombatManager.Instance.Combats[regID].Active)
            {
                // Если идёт бой, то перебрасываем войска сразу туда.
                CombatManager.Instance.AddCombatUnits(CombatManager.Instance.Combats[regID], forAttacker, milUnitID, amount, baseID);
            }
            else
            {
                // Если боя нет, то в пул войны.
                var warPool = forAttacker ? war.AttackerUnits : war.DefenderUnits;
                if (warPool.ContainsKey(baseID))
                {
                    if(warPool[baseID].ContainsKey(milUnitID))
                        warPool[baseID][milUnitID] += amount;
                    else
                        warPool[baseID][milUnitID] = amount;
                }
                else
                {
                    warPool.Add(baseID, new Dictionary<int, int>());
                    warPool[baseID][milUnitID] = amount;
                }
            }
        }

        /// <summary>
        ///  Контратака. Начинается через определённое количество ходов бездействия агрессора.
        /// </summary>
        /// <param name="war"></param>
        void CounterAttack(War war)
        {
            if (war.CompletedWarPhases.Count > 0)
            {
                WarPhase phase = war.CompletedWarPhases[war.CompletedWarPhases.Count - 1];
                BeginPhase(war, phase);
            }
        }
    }

    public class WarManager_DS
    {
        public Dictionary<int, War> Wars = new Dictionary<int, War>(); //Все идущие в данный момент войны (Key - индекс региона, где идёт война (Defender)).
    }

    public class War
    {
        public bool Active;
        public int RegionID;
        public int AggressorAuthorityID;
        public int InitiatorRadicals;
        public Dictionary<int, Dictionary<int, int>> AttackerUnits, DefenderUnits;  //<BaseID, <MilitaryUnitID, Amount>> (BaseID с какой базы прибыли войска: -1 - из домашнего пула; -11 - из морского пула)
        public int AttackerMoral, DefenderMoral;
        public int AttackerMoralPenalty, DefenderMoralPenalty;
        public List<WarPhase> CompletedWarPhases;
        public WarPhase CurrentPhase;
        public int NonePhaseTurns;  // Количество ходов без военных действий. Нужно для инициирования контратаки.
    }
}
