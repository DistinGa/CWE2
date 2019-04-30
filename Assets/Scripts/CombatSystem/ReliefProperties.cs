using System.Collections;
using System.Linq;
using System.Collections.Generic;
using Assets.SimpleLocalization;

namespace nsCombat
{
    /// <summary>
    /// Особенности рельефа (бонусы/панальти для боевых групп в бою)
    /// </summary>
    public class ReliefProperties
    {
        public int ID;
        public string _nameID;
        public List<WarPhasePenalty> _penalties;

        public string Name
        {
            get
            {
                return LocalizationManager.Localize(_nameID);
            }
        }

        public WarPhasePenalty GetClassPenalties(int classID, bool isAttacker)
        {
            WarPhasePenalty _res = new WarPhasePenalty();

            if (isAttacker)
            {
                foreach (var item in _penalties.Where(c => c.Area >= 0 && c.ClassIDs.Contains(classID)))
                {
                    _res.AddPenalties(item);
                }
            }
            else
            {
                foreach (var item in _penalties.Where(c => c.Area <= 0 && c.ClassIDs.Contains(classID)))
                {
                    _res.AddPenalties(item);
                }
            }

            return _res;
        }
    }
}
