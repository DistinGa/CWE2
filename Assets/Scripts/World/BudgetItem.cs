using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using nsEventSystem;

namespace nsWorld
{
    public class BudgetItem
    {
        public static readonly string BI_NatEconomy = "Nationalized economy";
        public static readonly string BI_PrivateSector = "Private sector";
        public static readonly string BI_Social = "Social";

        public string ID_Name;          //Имя для поиска элемента бюджета в списке
        public BudgetMinistry Ministry;
        public double Value;    // Денежные средства в данном департаменте бюджета
        public double WeeklyGrow = 0d;       //Еженедельный рост сектора (0-1%)
        public double LoadedWeeklyGrow = 0d; //Еженедельный рост загруженного сектора (0-1%)
        public float Factor = 1f;            //Коэффициент использования денежных средств (Например, для Private sector будет 0.5)
        public float DevLimitPct; // Лимит на использование бюджета для постройки/изучения (в процентах: 0-1)
        public List<Spends> Spends; // Очередь на постройку/изучение

        List<double> _history;

        public BudgetItem(string ID_Name, BudgetMinistry Ministry, float InitialLimit)
        {
            this.ID_Name = ID_Name;
            this.Ministry = Ministry;
            Value = 0d;
            DevLimitPct = InitialLimit;

            GameEventSystem.Subscribe(GameEventSystem.MyEventsTypes.DeleteSpends, DeleteSpends);
            GameEventSystem.Subscribe(GameEventSystem.MyEventsTypes.AddSpends, AddSpends);
            GameEventSystem.Subscribe(GameEventSystem.MyEventsTypes.NewYearEvents, NewYear);
        }

        ~BudgetItem()
        {
            GameEventSystem.UnSubscribe(GameEventSystem.MyEventsTypes.DeleteSpends, DeleteSpends);
            GameEventSystem.UnSubscribe(GameEventSystem.MyEventsTypes.AddSpends, AddSpends);
            GameEventSystem.UnSubscribe(GameEventSystem.MyEventsTypes.NewYearEvents, NewYear);
        }

        // Лимит на использование бюджета для постройки/изучения (в деньгах)
        public double DevLimit
        {
            get
            {
                return Value * DevLimitPct;
            }
        }

        private void AddSpends(object sender, EventArgs e)
        {
            Spends_EventArgs arg = e as Spends_EventArgs;
            Spends NewSpends = arg.SpendsRef;
            Spends.Add(NewSpends);
        }

        private void DeleteSpends(object sender, EventArgs e)
        {
            Spends_EventArgs arg = e as Spends_EventArgs;
            if(arg.BudgetItem == ID_Name)
                Spends.Remove(arg.SpendsRef);
        }

        private void NewYear(object sender, EventArgs e)
        {
            _history.Add(Value);
        }

        /// <summary>
        /// Распределение лимита на существующие заказы.
        /// </summary>
        public void DistributeFundsToSpends()
        {
            double funds = DevLimit;
            List<Spends> _SpendsCopy = Spends.ToList();

            foreach (var item in _SpendsCopy)
            {
                if (funds > 0)
                {
                    funds -= item.DoSpend(funds);
                }
            }
        }
    }

    public enum BudgetMinistry
    {
        Economy,
        Social
    }

}
