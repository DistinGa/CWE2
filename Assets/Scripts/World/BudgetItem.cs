using System.Collections;
using System.Collections.Generic;

namespace nsWorld
{
    public class BudgetItem
    {
        public static readonly string BI_NatEconomy = "Nationalized economy";
        public static readonly string BI_PrivateSector = "Private sector";
        public static readonly string BI_Social = "Social";

        public string ID_Name;          //Имя для поиска элемента бюджета в списке
        public BudgetMinistry Ministry;
        public double Value;
        public double WeeklyGrow = 0d;       //Еженедельный рост сектора (0-1%)
        public double LoadedWeeklyGrow = 0d; //Еженедельный рост загруженного сектора (0-1%)
        public float Factor = 1f;            //Во сколько раз дороже обходится использование ресурсов данного сектора

        public BudgetItem(string ID_Name, BudgetMinistry Ministry)
        {
            this.ID_Name = ID_Name;
            this.Ministry = Ministry;
            Value = 0d;
        }
    }

    public enum BudgetMinistry
    {
        Economy,
        Social
    }

}
