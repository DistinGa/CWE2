using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using nsEventSystem;

namespace nsWorld
{
    public class Region_Op
    {
        Region_Ds _RegData;
        RegionController _RegController;
        int _RegID;  //индекс в ассете Political Map
        int _SeaPoolID;

        public Region_Op(int RegID, int SeaPoolID, RegionController RegController, Region_Ds RegData)
        {
            _RegData = new Region_Ds();
            _RegController = RegController;
            _RegID = RegID;
            _SeaPoolID = SeaPoolID;
            _RegData = RegData;


            GameEventSystem.Subscribe(GameEventSystem.MyEventsTypes.TurnActions, OnTurn);
            GameEventSystem.Subscribe(GameEventSystem.MyEventsTypes.EndYearEvents, EndOfYear);
            GameEventSystem.Subscribe(GameEventSystem.MyEventsTypes.NewYearEvents, NewYear);
            GameEventSystem.Subscribe(GameEventSystem.MyEventsTypes.ChangeAuthority, OnChangeAuthority);
            GameEventSystem.Subscribe(GameEventSystem.MyEventsTypes.AddProsperity, OnAddProsperity);
        }

        ~Region_Op()
        {
            GameEventSystem.UnSubscribe(GameEventSystem.MyEventsTypes.TurnActions, OnTurn);
            GameEventSystem.UnSubscribe(GameEventSystem.MyEventsTypes.EndYearEvents, EndOfYear);
            GameEventSystem.UnSubscribe(GameEventSystem.MyEventsTypes.NewYearEvents, NewYear);
            GameEventSystem.UnSubscribe(GameEventSystem.MyEventsTypes.ChangeAuthority, OnChangeAuthority);
            GameEventSystem.UnSubscribe(GameEventSystem.MyEventsTypes.AddProsperity, OnAddProsperity);
        }

        public string RegName
        {
            get { return "Название из ассета"; }
        }

        public RegionController RegionController
        {
            get { return _RegController; }
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
                return _RegData.MilBaseID;
            }
        }

        public int ProsperityLevel
        {
            get { return _RegData.ProsperityLevel; }

            set
            {
                _RegData.ProsperityLevel = value;
                if (_RegData.ProsperityLevel < -ModEditor.ModProperties.Instance.ProspMaxValue)
                    _RegData.ProsperityLevel = -ModEditor.ModProperties.Instance.ProspMaxValue;
                if (_RegData.ProsperityLevel > ModEditor.ModProperties.Instance.ProspMaxValue)
                    _RegData.ProsperityLevel = ModEditor.ModProperties.Instance.ProspMaxValue;
            }
        }

        public int MovementValue
        {
            get { return _RegData.MovementValue; }
        }

        public int Moral
        {
            get { return _RegData.Moral; }

            private set { _RegData.Moral = value; }
        }

        public int Authority
        {
            get { return _RegData.Authority; }
        }

        public int GNP
        {
            get { return _RegData.GNP; }
        }

        /// <summary>
        /// Поочерёдный ход региона.
        /// Неконтролируемые выполняют свои действия, контролируемые передают управление ИИ или игроку
        /// </summary>
        public void MakeTurnRegion()
        {
            if (RegionController == null)
            {
                //Действия неконтролируемого региона

            }
            else
            {
                //Действия контролируемого региона
                RegionController.TurnStart();
            }

            //Участие в боях
            List<nsCombat.CombatData> combats = nsCombat.CombatManager.Instance.GetCombatsForReg(_RegID);
            foreach (var item in combats)
            {
                nsCombat.CombatManager.Instance.CommonTurn(item, _RegID);
            }
        }

        /// <summary>
        /// Ожидание хода и возврат "true", после окончания
        /// </summary>
        /// <returns></returns>
        public bool WaitTurnRegion()
        {
            if (RegionController == null)
            {
                return true;
            }
            else
            {
                return RegionController.TurnIsDone;
            }
        }

        /// <summary>
        /// Определяет является ли влияние режима максимальным
        /// </summary>
        /// <param name="AuthID">Индекс режима</param>
        /// <param name="ExcludeNeutral">Не проверять нейтральное влияние</param>
        /// <returns></returns>
        public bool IsMostBiggerInfluence(int AuthID, bool ExcludeNeutral = false)
        {
            bool res = true;
            int startInd = ExcludeNeutral ? 1 : 0;
            var maxVal = _RegData.Influence[AuthID]; //на случай, если тип значений Influence поменяется

            for (int i = startInd; i < _RegData.Influence.Count; i++)
            {
                if (i == AuthID)
                    continue;

                if (_RegData.Influence[i] >= maxVal)
                {
                    res = false;
                    break;
                }
            }

            return res;
        }

        /// <summary>
        /// Определяет является ли влияние режима минимальным
        /// </summary>
        /// <param name="AuthID">Индекс режима</param>
        /// <param name="ExcludeNeutral">Не проверять нейтральное влияние</param>
        /// <returns></returns>
        public bool IsMostSmallerInfluence(int AuthID, bool ExcludeNeutral = false)
        {
            bool res = true;
            int startInd = ExcludeNeutral ? 1 : 0;
            var minVal = _RegData.Influence[AuthID]; //на случай, если тип значений Influence поменяется

            for (int i = startInd; i < _RegData.Influence.Count; i++)
            {
                if (i == AuthID)
                    continue;

                if (_RegData.Influence[i] <= minVal)
                {
                    res = false;
                    break;
                }
            }

            return res;
        }

        private void OnTurn(object sender, EventArgs e)
        {
            ParlamentProcess();
            Moral += ProsperityLevel;
        }

        private void EndOfYear(object sender, EventArgs e)
        {
            if (_RegController == null)  //Только для неконтроллируемых регионов
                _RegData.GNPhistory.Add(_RegData.GNP);
        }

        private void NewYear(object sender, EventArgs e)
        {
            Random rnd = new Random();

            if (_RegController == null)  //Только для неконтролируемых регионов
            {
                // Изменение GNP
                int add = 0;
                if (_RegData.Authority == -1)
                {
                    add = rnd.Next(ModEditor.ModProperties.Instance.GNP_Neutral_Min, ModEditor.ModProperties.Instance.GNP_Neutral_Max + 1);
                }
                else
                {
                    if (World.TheWorld.GetRegionController(_RegData.Authority).ProsperityLevel > 0)
                        add = rnd.Next(ModEditor.ModProperties.Instance.GNP_HighDevLevel_Min, ModEditor.ModProperties.Instance.GNP_HighDevLevel_Max + 1);
                    else
                        add = rnd.Next(ModEditor.ModProperties.Instance.GNP_LowDevLevel_Min, ModEditor.ModProperties.Instance.GNP_LowDevLevel_Max + 1);
                }

                _RegData.GNP += add;

                // Постройка нейтральных юнитов

                // Популярность партий раз в год
                int _id = rnd.Next(ModEditor.ModProperties.Instance.PoliticParties.Count);
                float x = ModEditor.ModProperties.Instance.PoliticParties[_id].GetPartyPopularityGain(_RegController);
                AddPartyPopularity(_id, x);
            }

            // Для всех регионов.
            // Радикальные группы раз в год.
            int _idr = rnd.Next(_RegData.Radicals.Count);
            // Если попали на группу, соответствующую текущему режиму (не радикалы), меняем индекс.
            if (_idr == Authority)
            {
                if (_idr == _RegData.Radicals.Count - 1)
                    _idr -= 1;
                else _idr += 1;
            }
            AddRadicalsPopularity(_idr, ModEditor.ModProperties.Instance.AnnualRadicalsPopularityGain);
        }

        void ParlamentProcess()
        {
            for (int i = 0; i < _RegData.Parties.Count; i++)
            {
                var party = _RegData.Parties[i];
                var party_Prop = party.Party_Prop;

                //Принятие закона
                if (--party.RemainTurns <= 0)
                {
                    if (party.PoliticalLawIDs.Count > 0)
                    {
                        //Если у партии нет большинства в парламенте, то оппозиция блокирует закон Important
                        if (party.Popularity < 50f && party_Prop.LawIsImportant(party.PoliticalLawIDs[0]))
                            GameEventSystem.InvokeEvents(GameEventSystem.MyEventsTypes.AbortPartyLawInRegion
                                , new AbortPartyLawInRegion_EventArgs() { RegID = _RegID, PartyID = i });
                       else
                         {
                            party_Prop.PassLaw(party.PoliticalLawIDs[0]);
                            party.PoliticalLawIDs.RemoveAt(0);
                            PartyGetNewLaw(party);
                        }
                    }

                }
            }
        }

        void PartyGetNewLaw(PoliticParty party)
        {
            ModEditor.ModProperties modProperties = ModEditor.ModProperties.Instance;

            var party_Prop = party.Party_Prop;

            //Если в очереди не осталось законов, выбираем новый
            if (party.PoliticalLawIDs.Count == 0)
            {
                party.PoliticalLawIDs.Add(party_Prop.GetNewLawID(_RegController));
            }

            //Если партия на первом месте по популярности, закон принимается PassingLawTime1 ходов,
            //если на втором - PassingLawTime2 ходов,
            //остальные партии принимают законы PassingLawTime_ ходов.
            var plist = _RegData.Parties.OrderBy(p => p.Popularity).ToList();
            switch (plist.IndexOf(party))
            {
                case 0:
                    party.RemainTurns = modProperties.PassingLawTime1;
                    break;
                case 1:
                    party.RemainTurns = modProperties.PassingLawTime2;
                    break;
                default:
                    party.RemainTurns = modProperties.PassingLawTime_;
                    break;
            }
        }

        /// <summary>
        /// Отмена принятия закона оппозицией или игроком.
        /// </summary>
        /// <param name="party"></param>
        public void PartyAbortLaw(PoliticParty party)
        {
            party.PoliticalLawIDs.RemoveAt(0);
            PartyGetNewLaw(party);
        }

        /// <summary>
        /// Отмена принятия закона оппозицией или игроком.
        /// </summary>
        /// <param name="partyID">индекс партии</param>
        public void PartyAbortLaw(int partyID)
        {
            PartyAbortLaw(_RegData.Parties[partyID]);
        }

        /// <summary>
        /// Изменение популярности указанной партии. Популярность остальных партий соответственно корректируется.
        /// </summary>
        /// <param name="partieID">ID партии</param>
        /// <param name="amount">Величина изменения популярности</param>
        public void AddPartyPopularity(int partieID, float amount)
        {
            float actualAmount;
            int count = 0;

            if (amount > 0)
                actualAmount = Math.Min(amount, 100 - _RegData.Parties[partieID].Popularity);
            else
                actualAmount = Math.Min(amount, _RegData.Parties[partieID].Popularity);

            _RegData.Parties[partieID].Popularity += actualAmount;
            actualAmount *= -1f;

            //Добавленные проценты распределяем по оставшимся партиям (actualAmount уже имеет знак нужного действия)
            while (Math.Abs(actualAmount) > float.Epsilon)
            {
                // Сначала посчитаем, для скольки партий есть возможность добавления / отнимания популярности.
                count = 0;
                for (int i = 0; i < _RegData.Parties.Count; i++)
                {
                    var pp = _RegData.Parties[i];

                    if (actualAmount < 0f)
                    {
                        if (i != partieID && pp.Popularity > float.Epsilon)
                            count++;
                    }
                    else
                    {
                        if (i != partieID && pp.Popularity < 100f)
                            count++;
                    }
                }

                // Добавлять / отнимать будем стараться равное количество популярности.
                actualAmount = actualAmount / count;

                for (int i = 0; i < _RegData.Parties.Count; i++)
                {
                    var pp = _RegData.Parties[i];

                    if (actualAmount < 0f)
                    {
                        if (i != partieID && pp.Popularity > float.Epsilon)
                            pp.Popularity += actualAmount;
                    }
                    else
                    {
                        if (i != partieID && pp.Popularity < 100f)
                            pp.Popularity += actualAmount;
                    }
                }

                // Если где-то вылезли из диапазона 0 - 100, поправляем и считаем расхождение.
                actualAmount = 0;
                for (int i = 0; i < _RegData.Parties.Count; i++)
                {
                    var pp = _RegData.Parties[i];

                    if (pp.Popularity < 0f)
                    {
                        actualAmount += pp.Popularity;
                        pp.Popularity = 0f;
                    }
                    if (pp.Popularity > 100f)
                    {
                        actualAmount += (100f - pp.Popularity);
                        pp.Popularity = 100f;
                    }
                }

                //Если после распределения сумма популярностей всех партий не равна 100, расчитываем расхождение и повторяем итерацию.
                if (Math.Abs(actualAmount) < float.Epsilon)
                {
                    foreach (var item in _RegData.Parties)
                        actualAmount += item.Popularity;

                    actualAmount = 100 - actualAmount;
                }
            }
        }

        /// <summary>
        /// Изменение популярности указанной группы радикалов. Популярность остальных групп соответственно корректируется. (алгоритм идентичен AddPartyPopularity)
        /// </summary>
        /// <param name="partieID">ID группы радикалов</param>
        /// <param name="amount">Величина изменения популярности</param>
        public void AddRadicalsPopularity(int partieID, float amount)
        {
            float actualAmount;
            int count = 0;

            if (amount > 0)
                actualAmount = Math.Min(amount, 100 - _RegData.Radicals[partieID]);
            else
                actualAmount = Math.Min(amount, _RegData.Radicals[partieID]);

            _RegData.Radicals[partieID] += actualAmount;
            actualAmount *= -1f;

            //Добавленные проценты распределяем по оставшимся партиям (actualAmount уже имеет знак нужного действия)
            while (Math.Abs(actualAmount) > float.Epsilon)
            {
                // Сначала посчитаем, для скольки партий есть возможность добавления / отнимания популярности.
                count = 0;
                for (int i = 0; i < _RegData.Radicals.Count; i++)
                {
                    var pp = _RegData.Radicals[i];

                    if (actualAmount < 0f)
                    {
                        if (i != partieID && pp > float.Epsilon)
                            count++;
                    }
                    else
                    {
                        if (i != partieID && pp < 100f)
                            count++;
                    }
                }

                // Добавлять / отнимать будем стараться равное количество популярности.
                actualAmount = actualAmount / count;

                for (int i = 0; i < _RegData.Radicals.Count; i++)
                {
                    if (actualAmount < 0f)
                    {
                        if (i != partieID && _RegData.Radicals[i] > float.Epsilon)
                            _RegData.Radicals[i] += actualAmount;
                    }
                    else
                    {
                        if (i != partieID && _RegData.Radicals[i] < 100f)
                            _RegData.Radicals[i] += actualAmount;
                    }
                }

                // Если где-то вылезли из диапазона 0 - 100, поправляем и считаем расхождение.
                actualAmount = 0;
                for (int i = 0; i < _RegData.Radicals.Count; i++)
                {
                    if (_RegData.Radicals[i] < 0f)
                    {
                        actualAmount += _RegData.Radicals[i];
                        _RegData.Radicals[i] = 0f;
                    }
                    if (_RegData.Radicals[i] > 100f)
                    {
                        actualAmount += (100f - _RegData.Radicals[i]);
                        _RegData.Radicals[i] = 100f;
                    }
                }

                //Если после распределения сумма популярностей всех партий не равна 100, расчитываем расхождение и повторяем итерацию.
                if (Math.Abs(actualAmount) < float.Epsilon)
                {
                    foreach (var item in _RegData.Radicals)
                        actualAmount += item;

                    actualAmount = 100 - actualAmount;
                }
            }
        }

        public void AddInfluence(int InfID, float Amount)
        {
            if (Amount > 0)
                Amount = Math.Min(Amount, 100 - _RegData.Influence[InfID]);
            else
                Amount = Math.Max(Amount, -_RegData.Influence[InfID]);

            if (Amount == 0)
                return;

            _RegData.Influence[InfID] += Amount;

            //Дальше идёт компенсация: сначала отнимаем/прибавляем нейтральное влияние, потом поровну от остальных.
            int sumInf = 0;
            foreach (int inf in _RegData.Influence)
                sumInf += inf;

            //если получился перебор (или недобор), отнимаем излишки сначала от нейтрального влияния, потом равномерно от остальных.
            int rest = sumInf - 100;

            if (rest <= 0)
                return;

            int k = 0;  //количество распределяемое по оставшимся элемментам.
            int infcnt = _RegData.Influence.Where(x => x > 0).Count(); //Количество обрабатываемых элементов.
            if (_RegData.Influence[InfID] > 0)
                infcnt--;   //дополнительно отнимаем обрабатываемый элемент (переданный в качестве параметра метода)
            //Уменьшаем остальные влияния
            while (rest > 0)
            {
                k = rest / infcnt;  //infcnt не может равняться 0 по содержанию алгоритма
                if (k * infcnt < rest)
                    k++;    //округление

                for (int i = 0; i < _RegData.Influence.Count; i++)
                {
                    if (i == InfID || _RegData.Influence[i] == 0)
                        continue;

                    _RegData.Influence[i] -= k;
                    if (_RegData.Influence[i] < 0)
                        _RegData.Influence[i] = 0;
                }

                sumInf = 0;
                infcnt = (_RegData.Influence[InfID] > 0)? - 1: 0; //заранее отнимаем обрабатываемый элемент
                foreach (int inf in _RegData.Influence)
                {
                    sumInf += inf;
                    if (inf > 0)
                        infcnt++;
                }

                rest = sumInf - 100;
            }
        }

        //Старый вариант оставлен на всякий случай.
        //public void AddInfluence(int InfID, int Amount)
        //{
        //    if (Amount > 0)
        //        Amount = Math.Min(Amount, 100 - _RegData.Influence[InfID]);
        //    else
        //        Amount = Math.Max(Amount, -_RegData.Influence[InfID]);

        //    if (Amount == 0)
        //        return;

        //    _RegData.Influence[InfID] += Amount;

        //    //Дальше идёт компенсация: сначала отнимаем/прибавляем нейтральное влияние, потом поровну от остальных.
        //    int sumInf = 0;
        //    foreach (int inf in _RegData.Influence)
        //        sumInf += inf;

        //    //если получился перебор (или недобор), отнимаем излишки сначала от нейтрального влияния, потом равномерно от остальных.
        //    int rest = sumInf - 100;
        //    int infcnt = _RegData.Influence.Count - 1;  //Количество обрабатываемых элементов. Отнимаем изменяемый элемент

        //    if (rest == 0)
        //        return;

        //    if (InfID != 0)
        //    {
        //        infcnt--;   //элемент нейтрального влияния тоже отнимаем из количества компенсируемых элементов
        //        _RegData.Influence[0] -= rest;
        //        if (_RegData.Influence[0] < 0)
        //            rest = -_RegData.Influence[0];
        //        else
        //            return; //Если отняли от нейтрального влияния "в ноль", то заканчиваем. Добавить так, чтобы получилось > 100% в принципе нельзя.

        //        if (_RegData.Influence[0] < 0) _RegData.Influence[0] = 0;
        //        if (_RegData.Influence[0] > 100) _RegData.Influence[0] = 100;
        //    }

        //    int k = 0;  //количество распределяемое по оставшимся элемментам.
        //    //Уменьшаем остальные влияния
        //    while (rest > 0)
        //    {
        //        for (int i = 1; i < _RegData.Influence.Count; i++)
        //        {
        //            if (i == InfID)
        //                continue;

        //            k = rest / infcnt;
        //            if (k * infcnt < rest)
        //                k++;

        //            _RegData.Influence[i] -= k;
        //            if (_RegData.Influence[i] < 0)
        //                _RegData.Influence[i] = 0;

        //            rest -= k;
        //            infcnt--;
        //        }

        //        sumInf = 0;
        //        infcnt = 0;
        //        foreach (int inf in _RegData.Influence)
        //        {
        //            sumInf += inf;
        //            if(inf > 0)
        //                infcnt++;
        //        }

        //        rest = sumInf - 100;
        //    }

        //    //Увеличиваем остальные влияния
        //    if (rest < 0)
        //    {
        //        rest = -rest;
        //        for (int i = 1; i < _RegData.Influence.Count; i++)
        //        {
        //            if (i == InfID)
        //                continue;

        //            k = rest / infcnt;
        //            if (k * infcnt < rest)
        //                k++;

        //            _RegData.Influence[i] += k;

        //            rest -= k;
        //            infcnt--;
        //        }
        //    }

        //    sumInf = 0;
        //    foreach (int inf in _RegData.Influence)
        //    {
        //        sumInf += inf;
        //    }
        //    rest = sumInf - 100;

        //    if (rest != 0)
        //        throw new System.Exception("AddInfluence Exception!");
        //}

        public void AddProsperity(int Amount)
        {
            ProsperityLevel += Amount;
        }

        public void RegisterMilBase(int BaseID)
        {
            _RegData.MilBaseID = BaseID;
        }

        public float GetInfluence(int AuthID)
        {
            return _RegData.Influence[AuthID];
        }

        /// <summary>
        /// Смена режима.
        /// </summary>
        /// <param name="newAuthority"></param>
        void ChangeAuthority(int newAuthority)
        {
            // Смена радикальных групп.
            _RegData.Radicals[_RegData.Authority] = _RegData.Radicals[newAuthority];
            _RegData.Radicals[newAuthority] = 0;

            _RegData.Authority = newAuthority;
        }

        #region Events
        void OnChangeAuthority(object sender, EventArgs e)
        {
            ChangeAuthority_EventArgs _args = e as ChangeAuthority_EventArgs;

            if (_args.RegionID == _RegID)
            {
                ChangeAuthority(_args.NewAuthorityID);
            }
        }

        void OnAddProsperity(object sender, EventArgs e)
        {
            AddIntPropertyInRegion_EventArgs _args = e as AddIntPropertyInRegion_EventArgs;

            if (_args.RegionID == _RegID)
            {
                AddProsperity(_args.Amount);
            }
        }
        #endregion
    }

    public class Region_Ds: ISavable
    {
        public int MilBaseID = -1;
        public int Score;
        public int Authority;
        public List<float> Influence, Radicals;
        public int GNP;
        public List<int> GNPhistory;
        public int ProsperityLevel; //+-ProspMaxValue
        public List<PoliticParty> Parties;
        public int MovementValue;   //Насколько быстро военные юниты перемещаются по региону во время боя
        public int Moral;
        public float GNPToMilitaryPct;  //Процент GNP, который используется для постройки военных юнитов
    }
}
