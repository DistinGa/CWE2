using System;
using System.Collections.Generic;
using System.Linq;

namespace nsAI
{
    // Генетический алгоритм, который будет решать задачу о назначении целей для боя.
    class GeneticAlgorithm
    {
        const float SUVIVORSS_PERCENT = 0.3f;

        int survivorsCount;

        Random rnd;

        List<Sample> Population;
        int PopSize;
        int NumberOfPopulation;

        int[] TargetsValue;
        int[,] DamageMatrix;    // строки - вооружение, столбцы - цели
        int[] WeaponsFireCost; // "стоимость" выстрела данным оружием
        //int[] UnitID_WichWeaponOn;    // UnitID модели, на которой установлено оружие (служит для связи WeaponsFireCost и DictSupply через UnitID)
        Dictionary<int, int> DictSupply; // Key - UnitID, Value - supply
        Dictionary<int, List<int>> DictWeapons; // Key - UnitID, Value - список индексов оружия из массива WeaponsFireCost

        // weaponsID - массив индексов оружия
        // AttackersUnits - массив CombatUnit, на которых установлено оружие (количество и порядок элементов соответствует количеству и порядку в массиве weaponsID)
        public GeneticAlgorithm(int[,] damageMatrix, int[] targetsValue, int[] weaponsFireCost, Dictionary<int, List<int>> dictWeapons, Dictionary<int, int> dictSupply)
        {
            DamageMatrix = damageMatrix;
            TargetsValue = targetsValue;
            WeaponsFireCost = weaponsFireCost;
            DictWeapons = dictWeapons;
            DictSupply = dictSupply;

            PopSize = 1 + (int)(2 * Math.Log(Math.Pow((double)TargetsValue.Length, (double)weaponsFireCost.Length), 2d));
            survivorsCount = 1 + (int)Math.Round(PopSize * SUVIVORSS_PERCENT, 0);

            rnd = new Random();
        }

        /// <summary>
        /// Решение
        /// </summary>
        /// <param name="quantity">максимальное число поколений</param>
        /// <returns></returns>
        public int[] GetSolution(int quantity, bool optimize = false)
        {
            Genesis(PopSize);
            NumberOfPopulation = 1;

            Reproduction();

            while (NumberOfPopulation < quantity)
            {
                if (NumberOfPopulation % 20 == 0)
                {
                    if (optimize)
                    {
                        if (Population[0].Equals(Population[survivorsCount-1]))
                            break;
                    }
                }
                Reproduction();
            }

            return Population[0].Genome;
        }

        /// <summary>
        /// Создание стартовой популяции.
        /// </summary>
        void Genesis(int popSize)
        {
            int sampleSize = WeaponsFireCost.Length;
            int targetsCount = TargetsValue.Length;

            Population = new List<Sample>(popSize);

            for (int i = 0; i < popSize; i++)
            {
                Population.Add(GetRandomSample(sampleSize, targetsCount));
            }
        }

        Sample GetRandomSample(int sampleSize, int targetsCount)
        {
            var s = new Sample(sampleSize);

            for (int j = 0; j < sampleSize; j++)
            {
                s.Genome[j] = rnd.Next(targetsCount);
            }

            ValidateSample(ref s);
            return s;
        }

        /// <summary>
        /// Осуществляет проверку особи на возможность (проверяет наличие боеприпасов, если на модели установлено несколько видов оружия).
        /// </summary>
        /// <param name="s"></param>
        void ValidateSample(ref Sample s)
        {
            int _cuID;
            int _demand;

            foreach (var DictWeaponsItem in DictWeapons)
            {
                _cuID = DictWeaponsItem.Key;

                // Рассматриваем модели с несколькими видами оружия на борту.
                if (DictWeaponsItem.Value.Count > 1)
                {
                    _demand = 0;
                    // Считаем необходимый supply
                    foreach (var weaponIndx in DictWeaponsItem.Value)
                    {
                        if (s.Genome[weaponIndx] != -1)
                            _demand += WeaponsFireCost[weaponIndx];
                    }

                    if (_demand > DictSupply[_cuID])
                    {
                        // Если стрелять из всего оружия, установленного на моделях данного CombatUnit, то патронов не хватит.
                        // Для выбора из чего стрелять решаем "задачу о ранце" с использованием "жадного алгоритма".
                        // Сначала создаём список вооружения отсортированный по удельной поражающей способности (damage/firecost) в порядке убывания
                        Dictionary<int, float> _WeaponsAndFireCost = new Dictionary<int, float>(); // Key - индекс оружия; Value - удельная поражающая способность выстрела = damage/firecost
                        foreach (var weaponIndx in DictWeaponsItem.Value)
                        {
                            if(s.Genome[weaponIndx] >= 0)
                                _WeaponsAndFireCost.Add(weaponIndx, (float)DamageMatrix[weaponIndx, s.Genome[weaponIndx]] / (float)WeaponsFireCost[weaponIndx]);
                        }
                        //List<int> _sortedWeaponsByFireCost = _WeaponsAndFireCost.OrderByDescending(kv => kv.Value).Select(kv => kv.Key).ToList();

                        List<int> _sortedWeaponsByFireCost = new List<int>();
                        float _tmpVal = 0;
                        foreach (var item in _WeaponsAndFireCost)
                        {
                            if (item.Value > _tmpVal)
                            {
                                _sortedWeaponsByFireCost.Insert(0, item.Key);
                                _tmpVal = item.Value;
                            }
                            else
                                _sortedWeaponsByFireCost.Add(item.Key);
                        }

                        // Отбираем выстрелы в пределах имеющегося supply.
                        _demand = 0;
                        foreach (var weaponIndx in _sortedWeaponsByFireCost)
                        {
                            _demand += WeaponsFireCost[weaponIndx];
                            // Если превысили имеющийся у CombatUnit supply, из последующих видов оружия не стреляем.
                            if (_demand > DictSupply[_cuID])
                                s.Genome[weaponIndx] = -1;
                        }
                    }
                }
            }
        }

        void SortByEquity()
        {
            for (int p = 0; p < Population.Count; p++)
            {
                if (Population[p].Equity == 0)
                {
                    var popMember = Population[p];
                    popMember.Equity = EquityCalculations(popMember.Genome);
                    Population[p] = popMember;
                }
            }

            Population = Population.OrderByDescending(p => p.Equity).ToList();
        }

        /// <summary>
        /// Расчет эффективности особей.
        /// </summary>
        int EquityCalculations(int[] genome)
        {
            int eq = 0;
            int[] damages = new int[TargetsValue.Length];

            // Считаем урон целей.
            for (int i = 0; i < genome.Length; i++)
            {
                if(genome[i] >= 0)
                    damages[genome[i]] += DamageMatrix[i, genome[i]];
            }

            // Убираем "переуничтожение", когда урона нанесено больше, чем было возможно.
            for (int i = 0; i < TargetsValue.Length; i++)
            {
                if (damages[i] >= TargetsValue[i])
                {
                    damages[i] = TargetsValue[i];
                    eq++;   // Бонус за уничтожение цели.
                }
                eq += damages[i];
            }

            return eq;
        }

        /// <summary>
        /// Создание потомка.
        /// </summary>
        /// <param name="s1"></param>
        /// <param name="s2"></param>
        /// <param name="withStrength">Учитывать ли силу генов.</param>
        /// <returns></returns>
        Sample GenerateChild(Sample s1, Sample s2)
        {
            Sample child = new Sample(s1.Genome.Length);

            for (int i = 0; i < s1.Genome.Length; i++)
            {
                if (rnd.Next(100) < rnd.Next(100))
                {
                    child.Genome[i] = s2.Genome[i];
                }
                else
                {
                    child.Genome[i] = s1.Genome[i];
                }
            }

            ValidateSample(ref child);
            return child;
        }

        /// <summary>
        /// Генерация следующего поколения.
        /// </summary>
        void Reproduction()
        {
            SortByEquity();
            Population.RemoveRange(survivorsCount, Population.Count - survivorsCount);

            // Добавляем несколько случайных особей (вместо мутации).
            var newRndCount = survivorsCount >> 1;
            if (newRndCount == 0) newRndCount++;

            for (int i = 0; i < newRndCount; i++)
            {
                Population.Add(GetRandomSample(WeaponsFireCost.Length, TargetsValue.Length));
            }

            // Отбор родительских пар.
            int _popCount = Population.Count;
            int _turnCount = (int)((1 - SUVIVORSS_PERCENT) / SUVIVORSS_PERCENT);
            for (int turn = 0; turn < _turnCount; turn++)
            {
                for (int i = 0; i < _popCount; i++)
                {
                    var _tmpInt = rnd.Next(_popCount);
                    if (_tmpInt == i)
                    {
                        if (_tmpInt == _popCount - 1)
                            _tmpInt--;
                        else
                            _tmpInt++;
                    }


                    if (!Population[i].Equals(Population[_tmpInt]))
                        Population.Add(GenerateChild(Population[i], Population[_tmpInt]));
                }
            }

            NumberOfPopulation++;
        }
    }

    /// <summary>
    /// Особь.
    /// </summary>
    struct Sample
    {
        public int Equity;
        // Геном - размер = количеству задействованного вооружения, индекс = индексу оружия, значение = индексу атакуемой цели (-1 - не атакуем этим оружием).
        public int[] Genome;

        public Sample(int length)
        {
            Equity = 0;
            Genome = new int[length];
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Sample))
            {
                return false;
            }

            var sample = (Sample)obj;
            bool _tmp = true;
            for (int i = 0; i < Genome.Length; i++)
            {
                if (Genome[i] != sample.Genome[i])
                {
                    _tmp = false;
                    break;
                }
            }
            return _tmp;
        }

        public override int GetHashCode()
        {
            return -1558141132 + EqualityComparer<int[]>.Default.GetHashCode(Genome);
        }
    }
}
