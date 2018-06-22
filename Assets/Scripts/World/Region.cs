using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace World
{
    public class Region_Op
    {
        Region_Ds _RegData;
        RegionController _RegController;
        int _RegID;  //индекс в ассете Political Map
        int _SeaPoolID;
        List<Sprite> _Flags;    //индекс соответствует индексу Authorities

        public Region_Op(Region_Ds RegData, RegionController RegController, int RegID, int SeaPoolID, List<Sprite> Flags)
        {
            _RegData = RegData;
            _RegController = RegController;
            _RegID = RegID;
            _SeaPoolID = SeaPoolID;
            _Flags = Flags;

            GameEventSystem.Instance.SubscribeOnTurn(Turn);
            GameEventSystem.Instance.SubscribeOnEndYear(EndOfYear);
            GameEventSystem.Instance.SubscribeOnNewYear(NewYear);
        }

        ~Region_Op()
        {
            GameEventSystem.Instance.SubscribeOnTurn(Turn, false);
            GameEventSystem.Instance.SubscribeOnEndYear(EndOfYear, false);
            GameEventSystem.Instance.SubscribeOnNewYear(NewYear, false);
        }

        public string RegName
        {
            get { return "Название из ассета"; }
        }

        public int SeaPoolID
        {
            get
            {
                return _SeaPoolID;
            }
        }

        public int MilitaryBaseID
        {
            get
            {
                return _RegData._MilBaseID;
            }
        }

        private void Turn()
        {

        }

        private void EndOfYear()
        {
            if (_RegController != null)  //Только для неконтроллируемых регионов
                _RegData.GNPhistory.Add(_RegData.GNP);
        }

        private void NewYear()
        {
            if (_RegController != null)  //Только для неконтролируемых регионов
            {
                //Изменение GNP
                int add = 0;
                if (_RegData.Authority == 0)
                {
                    add = Random.Range(ModEditor.ModProperties.Instance.GNP_Neutral_Min, ModEditor.ModProperties.Instance.GNP_Neutral_Max + 1);
                }
                else
                {
                    if (_RegData.ProsperityLevel > 0)
                        add = Random.Range(ModEditor.ModProperties.Instance.GNP_HighDevLevel_Min, ModEditor.ModProperties.Instance.GNP_HighDevLevel_Max + 1);
                    else
                        add = Random.Range(ModEditor.ModProperties.Instance.GNP_LowDevLevel_Min, ModEditor.ModProperties.Instance.GNP_LowDevLevel_Max + 1);
                }

                _RegData.GNP += add;
            }
        }

        public void AddInfluence(int InfID, int Amount)
        {
            if (Amount > 0)
                Amount = Mathf.Min(Amount, 100 - _RegData.Influence[InfID]);
            else
                Amount = Mathf.Max(Amount, -_RegData.Influence[InfID]);

            if (Amount == 0)
                return;

            _RegData.Influence[InfID] += Amount;

            //Дальше идёт компенсация: сначала отнимаем/прибавляем нейтральное влияние, потом поровну от остальных.
            int sumInf = 0;
            foreach (int inf in _RegData.Influence)
                sumInf += inf;

            //если получился перебор (или недобор), отнимаем излишки сначала от нейтрального влияния, потом равномерно от остальных.
            int rest = sumInf - 100;
            int infcnt = _RegData.Influence.Count - 1;  //Количество обрабатываемых элементов. Отнимаем изменяемый элемент

            if (rest == 0)
                return;

            if (InfID != 0)
            {
                infcnt--;   //элемент нейтрального влияния тоже отнимаем из количества компенсируемых элементов
                _RegData.Influence[0] -= rest;
                if (_RegData.Influence[0] < 0)
                    rest = -_RegData.Influence[0];
                else
                    return; //Если отняли от нейтрального влияния "в ноль", то заканчиваем. Добавить так, чтобы получилось > 100% в принципе нельзя.

                Mathf.Clamp(_RegData.Influence[0], 0, 100);
            }

            int k = 0;  //количество распределяемое по оставшимся элемментам.
            //Уменьшаем остальные влияния
            while (rest > 0)
            {
                for (int i = 1; i < _RegData.Influence.Count; i++)
                {
                    if (i == InfID)
                        continue;

                    k = rest / infcnt;
                    if (k * infcnt < rest)
                        k++;

                    _RegData.Influence[i] -= k;
                    if (_RegData.Influence[i] < 0)
                        _RegData.Influence[i] = 0;

                    rest -= k;
                    infcnt--;
                }

                sumInf = 0;
                infcnt = 0;
                foreach (int inf in _RegData.Influence)
                {
                    sumInf += inf;
                    if(inf > 0)
                        infcnt++;
                }

                rest = sumInf - 100;
            }

            //Увеличиваем остальные влияния
            if (rest < 0)
            {
                rest = -rest;
                for (int i = 1; i < _RegData.Influence.Count; i++)
                {
                    if (i == InfID)
                        continue;

                    k = rest / infcnt;
                    if (k * infcnt < rest)
                        k++;

                    _RegData.Influence[i] += k;

                    rest -= k;
                    infcnt--;
                }
            }

            sumInf = 0;
            foreach (int inf in _RegData.Influence)
            {
                sumInf += inf;
            }
            rest = sumInf - 100;

            if (rest != 0)
                throw new System.Exception("AddInfluence Exception!");
        }

        public void AddProsperity(int Amount)
        {
            _RegData.ProsperityLevel += Amount;
            _RegData.ProsperityLevel = Mathf.Clamp(_RegData.ProsperityLevel, -ModEditor.ModProperties.Instance.RadProspMaxValue, ModEditor.ModProperties.Instance.RadProspMaxValue);
        }

        public void AddSpy(int AuthID, int Amount)
        {
            _RegData.Spies[AuthID] += Amount;
        }

        public void RegisterMilBase(int BaseID)
        {
            _RegData._MilBaseID = BaseID;
        }
    }

    public class Region_Ds
    {
        public int _MilBaseID;
        public int Score;
        public int Authority, OppAuthority;
        public List<int> Influence, Spies;
        public Dictionary<int, int> GovForces, OppForces;  //Key - MilitaryUnit ID; Value - amount
        public int GNP;
        public List<int> GNPhistory;
        public int ProsperityLevel;

    }
}
