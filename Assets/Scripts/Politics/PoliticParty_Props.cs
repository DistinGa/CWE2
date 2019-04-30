using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ModEditor
{
    //От чего зависит рост популярноссти партии
    public enum PoliticPartyType
    {
        NatFundGap, //дефицит в нацфонде
        Prosperity, //уровень благосостояния < 0
        CorInf,     //коррупция + инфляция > 0
        SelfInfluenceMostBigger,    //своё влияние больше остальныхх
        SelfInfluenceMostSmaller    //своё влияние меньше остальных
    }

    public class PoliticParty_Props
    {
        string _Name;
        PoliticPartyType _PartyType;
        List<PoliticLaw_Props> _Laws; //Список доступных для партии законов

        public PoliticParty_Props(string Name, PoliticPartyType PartyType, List<PoliticLaw_Props> Laws)
        {
            _Name = Name;
            _PartyType = PartyType;
            _Laws = Laws;
        }

        public string Name
        {
            get { return Assets.SimpleLocalization.LocalizationManager.Localize(_Name); }
        }

        public PoliticPartyType PartyType
        {
            get { return PartyType; }
        }

        /// <summary>
        /// Выбрать следующий закон для принятия.
        /// Если регион контролируемый, выбирается любой закон, если неконролируемый - только не Important.
        /// </summary>
        /// <param name="RC"></param>
        /// <returns></returns>
        public int GetNewLawID(RegionController RC = null)
        {
            List<PoliticLaw_Props> laws;

            if (RC == null)
            {
                laws = _Laws.Where(l => (!l._Important && l.Condition.CheckCondition())).ToList();
            }
            else
            {
                laws = _Laws.Where(l => l.Condition.CheckCondition()).ToList();
            }

            int ind = (new Random()).Next(_Laws.Count);
            return _Laws.IndexOf(laws[ind]);
        }

        public void PassLaw(int ID)
        {
            _Laws[ID].PassingLaw();
        }

        /// <summary>
        /// Рост популярности партии в регионе
        /// </summary>
        /// <param name="RegionController"></param>
        /// <returns></returns>
        public float GetPartyPopularityGain(RegionController RegionController)
        {
            //В неконтролируемых регионах популярность меняется случайным образом раз в год, поэтому на YEAR_TURNS_COUNT не делим.
            if (RegionController == null)
            {
                return ModProperties.Instance.AnnualPartyPopularityGain;
            }

            //Для контролируемых стран - каждый ход
            double res = 0; //double для Math.Round

            switch (_PartyType)
            {
                case PoliticPartyType.NatFundGap:
                    if (RegionController.NatFund < 0)
                    {
                        res = Math.Round((RegionController.NatFund / ModProperties.Instance.RelativeNatFundToPartyGrow) * ModProperties.Instance.AnnualPartyPopularityGain / ModProperties.YEAR_TURNS_COUNT);
                    }
                    break;
                case PoliticPartyType.Prosperity:
                    if (RegionController.ProsperityLevel < 0)
                    {
                        res = Math.Round(-(float)RegionController.ProsperityLevel / ModProperties.Instance.ProspMaxValue * ModProperties.Instance.AnnualPartyPopularityGain / ModProperties.YEAR_TURNS_COUNT);
                    }
                    break;
                case PoliticPartyType.CorInf:
                    int CorInf = RegionController.Corruption + RegionController.Inflation;
                    if (CorInf > 0)
                    {
                        res = Math.Round(CorInf / 200f * ModProperties.Instance.AnnualPartyPopularityGain / ModProperties.YEAR_TURNS_COUNT);
                    }
                    break;
                case PoliticPartyType.SelfInfluenceMostBigger:
                    if (RegionController.ControlledRegion.IsMostBiggerInfluence(RegionController.AuthorityID))
                        res = ModProperties.Instance.AnnualPartyPopularityGain / ModProperties.YEAR_TURNS_COUNT;
                    break;
                case PoliticPartyType.SelfInfluenceMostSmaller:
                    if (RegionController.ControlledRegion.IsMostSmallerInfluence(RegionController.AuthorityID))
                        res = ModProperties.Instance.AnnualPartyPopularityGain / ModProperties.YEAR_TURNS_COUNT;
                    break;
                default:
                    break;
            }

            return (float)res;
        }

        public bool LawIsImportant(int lawID)
        {
            return _Laws[lawID]._Important;
        }
    }

    public class PoliticLaw_Props
    {
        public bool _Important;
        public IConditional Condition;
        List<nsEventSystem.GameEvent> GameEvent;

        public void PassingLaw()
        {
            foreach (var item in GameEvent)
                nsEventSystem.GameEventSystem.InvokeEvents(item.EventType, item.EventArgs);
        }
    }

}
