using System;
using System.Collections;
using System.Collections.Generic;
using nsEventSystem;

namespace nsMilitary
{
    public class MilitaryPool
    {
        public bool Active;
        protected int _Capacity;
        protected Dictionary<int, int> _MilForces;  //Key - MilitaryUnit ID; Value - amount

        public MilitaryPool()
        {
            _MilForces = new Dictionary<int, int>();
        }

        public Dictionary<int, int> GetUnits()
        {
            return _MilForces;
        }

        //Возвращает количество определённых юнитов
        public int GetUnitsAmount(int indx)
        {
            return _MilForces[indx];
        }

        //Возвращает общее количество всех военных юнитов в пуле
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

            if (Amount < 0)
            {
                if(_MilForces[UnitID] == 0)
                    _MilForces.Remove(UnitID);

                if (_MilForces[UnitID] < 0)
                    throw new Exception("Subtract too many units from pool.");
            }
        }

        //Вместимость базы (морского пула для сухопутных юнитов)
        public int Capacity
        {
            get { return _Capacity; }
        }

        //Свободное место на базе (d морском пуле для сухопутных юнитов)
        public int FreeCapacity
        {
            get
            {
                if (Active)
                    return Capacity - GetUnitsAmount();
                else
                    return 0;
            }
        }

        public void AddCapacity(int Amount)
        {
            _Capacity += Amount;
        }
    }

    public class SeaPool
    {
        List<MilitaryPool> _NavyList; //Список флотов контролируемых стран. Index - Authority

        public SeaPool()
        {
            _NavyList = new List<MilitaryPool>();
        }

        public MilitaryPool GetNavy(int Authority)
        {
            return _NavyList[Authority];
        }
    }

    public class MilitaryBase: MilitaryPool
    {
        int _RegID;
        int _AuthID;
        int _Capacity;

        public MilitaryBase(int RegID, int AuthID, int Capacity)
        {
            _RegID = RegID;
            _AuthID = AuthID;
            _Capacity = Capacity;
            _MilForces = new Dictionary<int, int>();
        }

        public int AuthID
        {
            set { _AuthID = value; }
            get { return _AuthID; }
        }
    }

    public class UnitOnTheWay
    {
        int _Authority; //Чьи это юниты
        DestinationTypes _DestType;
        int _DestID;
        int _UnitID;
        int _Amount;
        int _LifeTime;
        Action<UnitOnTheWay> _Deleter;

        public UnitOnTheWay(int Authority, DestinationTypes DestType, int DestID, int UnitID, int Amount, int LifeTime, Action<UnitOnTheWay> Deleter)
        {
            _Authority = Authority;
            _DestType = DestType;
            _DestID = DestID;
            _UnitID = UnitID;
            _Amount = Amount;
            _LifeTime = LifeTime;
            _Deleter = Deleter;

            GameEventSystem.Subscribe(GameEventSystem.MyEventsTypes.TurnActions, OnTurn);
        }

        ~UnitOnTheWay()
        {
            GameEventSystem.UnSubscribe(GameEventSystem.MyEventsTypes.TurnActions, OnTurn);
        }

        void OnTurn(object sender, EventArgs e)
        {
            if (--_LifeTime == 0)
                ExecuteMovement();
        }

        void ExecuteMovement()
        {
            switch (_DestType)
            {
                case DestinationTypes.MainPool:
                    MilitaryManager.Instance.GetMainMilPool(_DestID).AddUnits(_UnitID, _Amount);
                    break;
                case DestinationTypes.SeaPool:
                    MilitaryManager.Instance.GetSeaPool(_DestID).GetNavy(_Authority).AddUnits(_UnitID, _Amount);
                    break;
                case DestinationTypes.MilitaryBase:
                    MilitaryBase mb = MilitaryManager.Instance.GetMilitaryBase(_DestID);
                    if (mb.FreeCapacity >= _Amount)
                        mb.AddUnits(_UnitID, _Amount);
                    else
                    {
                        //Юнитов пришло больше, чем свободного места на базе. Заполняем базу, "лишних" юнитов отправляем в основной пул.
                        int fc = mb.FreeCapacity;
                        mb.AddUnits(_UnitID, fc);
                        _LifeTime = MilitaryManager.Instance.GetMovementTime(_DestType, _DestID, DestinationTypes.MainPool, _Authority);
                        _DestType = DestinationTypes.MainPool;
                        _DestID = mb.AuthID;
                        _Amount -= fc;
                    }
                    break;
                case DestinationTypes.War:

                    break;
                default:
                    break;
            }

            if (_Deleter != null && _LifeTime == 0)
                _Deleter(this);
        }
    }

    public enum DestinationTypes
    {
        MainPool,
        SeaPool,
        MilitaryBase,
        War
    }
}
