using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using nsWorld;
using nsEventSystem;

namespace nsWorld
{
    /// <summary>
    /// Заказ на постройку юнитов (апгрейд военных систем или изучение технологии)
    /// </summary>
    public class Spends
    {
        int _Authority;
        SpendsSubjects _Subject;
        string _BudgetItem;
        SpendsGoals _Goal;
        int _GoalAmount;    //Размер заказа (сколько нужно построить)
        double _TurnSpends; //траты за ход
        double _Accumulation;   //уже накоплено
        int _ID; //ID юнита, технологии или апгрейда
        RegionController _RC = null;

        public Spends(int Authority, SpendsSubjects Subject, string Type, SpendsGoals Goal, double TurnSpends, int ObjID, int GoalAmount, double Accumulation = 0)
        {
            _Authority = Authority;
            _Subject = Subject;
            _BudgetItem = Type;
            _Goal = Goal;
            _TurnSpends = TurnSpends;
            _ID = ObjID;
            _GoalAmount = GoalAmount;
            _Accumulation = Accumulation;
        }

        private RegionController RegionController
        {
            get
            {
                if (_RC == null)
                    _RC = World.TheWorld.GetRegionController(_Authority);

                return _RC;
            }
        }

        public string SpendsType
        {
            set
            {
                if (_BudgetItem == value)
                    return;

                if (RegionController.GetBudgetItem(_BudgetItem).Ministry != RegionController.GetBudgetItem(value).Ministry)
                {
                    throw new Exception("Incorrect change of SpendsType. Budget ministries are different.");
                }

                //Если меняем источник инвестирования накопленную сумму пропорционально меняем. В итоге процент завершённости юнита остаётся прежним, изменяется стоимость "остаточной постройки"
                _Accumulation = _Accumulation / RegionController.GetBudgetItem(_BudgetItem).Factor * RegionController.GetBudgetItem(value).Factor;

                // Удаление из старой очереди.
                GameEventSystem.InvokeEvents(GameEventSystem.MyEventsTypes.DeleteSpends, new Spends_EventArgs() { BudgetItem = _BudgetItem, SpendsRef = this });

                _BudgetItem = value;

                // Добавление в новую очередь.
                GameEventSystem.InvokeEvents(GameEventSystem.MyEventsTypes.AddSpends, new Spends_EventArgs() { BudgetItem = _BudgetItem, SpendsRef = this });
            }
            get { return _BudgetItem; }
        }

        //Определение стоимости юнита, технологии или апгрейда
        public double Cost
        {
            get
            {
                double res = 0;

                if (_Goal == SpendsGoals.MilitaryUnit)
                    res = nsMilitary.MilitaryManager.Instance.GetMilitaryUnit(_ID).Cost;
                else if (_Goal == SpendsGoals.CosmoUnit)
                    res = 0;
                else
                    res = 0;

                return res * RegionController.GetBudgetItem(_BudgetItem).Factor;
            }
        }

        public double Accumulation
        {
            get { return _Accumulation; }
        }

        public double TurnSpends
        {
            set { _TurnSpends = value; }
            get { return _TurnSpends; }
        }

        /// <summary>
        /// Возвращает использованное количество денежных средств.
        /// </summary>
        /// <param name="SpendsAmount"></param>
        /// <returns></returns>
        public double DoSpend(double SpendsAmount = 0d)
        {
            if (SpendsAmount < 0d)
                throw new Exception("Negative SpendsAmount");

            if(SpendsAmount <= double.Epsilon)
                SpendsAmount = _TurnSpends;

            SpendsAmount = Math.Min(SpendsAmount, _TurnSpends);
            SpendsAmount = Math.Min(SpendsAmount, _GoalAmount * Cost - Accumulation);

            _Accumulation += SpendsAmount;
            Execute();

            return SpendsAmount;
        }

        void Execute()
        {
            if (_Accumulation < Cost)
                return;

            GameEventSystem.SpendingComplete(_Subject, _ID, _Authority);

            _GoalAmount--;
            _Accumulation -= Cost;

            if (_GoalAmount > 0)
                Execute();
            else
                // Заказ выполнен, удаляем из списка.
                GameEventSystem.InvokeEvents(GameEventSystem.MyEventsTypes.DeleteSpends, new Spends_EventArgs() { BudgetItem = _BudgetItem, SpendsRef = this });
        }
    }
}
