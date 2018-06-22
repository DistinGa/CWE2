using System;
using System.Collections;
using System.Collections.Generic;

namespace World
{
    public class MilitariPool
    {
        Dictionary<int, int> _MilForces;  //Key - MilitaryUnit ID; Value - amount

        public Dictionary<int, int> GetUnits()
        {
            return _MilForces;
        }

        //Возвращает количество определённых юнитов
        public int GetUnitsAmount(int indx)
        {
            return _MilForces[indx];
        }

        //Возвращает количество всех военны юнитов в пуле
        public int GetUnitsAmount()
        {
            int res = 0;
            foreach (var item in _MilForces)
            {
                res += item.Value;
            }
            return res;
        }

        public void AddUnits(int UnitID, int Amount)
        {
            if (_MilForces.ContainsKey(UnitID))
                _MilForces[UnitID] += Amount;
            else
                _MilForces.Add(UnitID, Amount);
        }
    }

    public class SeaPool: MilitariPool
    {
        Dictionary<int, int> _MilForces;  //Key - MilitaryUnit ID; Value - amount

    }

    public class MilitaryBase: MilitariPool
    {
        int _RegID;
        int _AuthID;
        int _Capacity;
        Dictionary<int, int> _MilForces;  //Key - MilitaryUnit ID; Value - amount

        public MilitaryBase(int RegID, int AuthID, int Capacity)
        {
            _RegID = RegID;
            _AuthID = AuthID;
            _Capacity = Capacity;
            _MilForces = new Dictionary<int, int>();
        }

        //Вместимость базы
        public int Capacity
        {
            get { return _Capacity; }
        }

        //Свободное место в базе
        public int FreeCapacity
        {
            get { return Capacity - GetUnitsAmount(); }
        }

        public void AddCapacity(int Amount)
        {
            _Capacity += Amount;
        }
    }

    public class UnitOnTheWay
    {
        DestinationTypes _DestType;
        int _DestID;
        int _UnitID;
        int _Amount;
        int _LifeTime;
        Action<UnitOnTheWay> _Deleter;

        public UnitOnTheWay(DestinationTypes DestType, int DestID, int UnitID, int Amount, int LifeTime, Action<UnitOnTheWay> Deleter)
        {
            _DestType = DestType;
            _DestID = DestID;
            _UnitID = UnitID;
            _Amount = Amount;
            _LifeTime = LifeTime;
            _Deleter = Deleter;

            GameEventSystem.Instance.SubscribeOnTurn(OnTurn);
        }

        ~UnitOnTheWay()
        {
            GameEventSystem.Instance.SubscribeOnTurn(OnTurn, false);
        }

        void OnTurn()
        {
            if (--_LifeTime == 0)
                ExecuteMovement();
        }

        void ExecuteMovement()
        {
            switch (_DestType)
            {
                case DestinationTypes.MainPool:
                    World.TheWorld.GetMainMilPool(_DestID).AddUnits(_UnitID, _Amount);
                    break;
                case DestinationTypes.SeaPool:
                    World.TheWorld.GetSeaPool(_DestID).AddUnits(_UnitID, _Amount);
                    break;
                case DestinationTypes.MilitaryBase:
                    World.TheWorld.GetMilitaryBase(_DestID).AddUnits(_UnitID, _Amount);
                    break;
                case DestinationTypes.Region:

                    break;
                default:
                    break;
            }

            if (_Deleter != null)
                _Deleter(this);
        }
    }

    public enum DestinationTypes
    {
        MainPool,
        SeaPool,
        MilitaryBase,
        Region
    }
}
