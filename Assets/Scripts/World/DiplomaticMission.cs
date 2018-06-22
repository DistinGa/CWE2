using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace World
{
    public class DiplomaticMission
    {
        int _AuthID;
        int _RegID;
        int _LifeTime;
        int _Parameter;
        DipMissionType _Type;

        public DiplomaticMission(DipMissionType Type, int AuthID, int RegID, int Parameter, int LifeTime)
        {
            _AuthID = AuthID;
            _RegID = RegID;
            _LifeTime = LifeTime;
            _Parameter = Parameter;
            _Type = Type;

            GameEventSystem.Instance.SubscribeOnTurn(OnTurn);
        }

        ~DiplomaticMission()
        {
            GameEventSystem.Instance.SubscribeOnTurn(OnTurn, false);
        }

        public int LifeTime
        {
            get{ return _LifeTime; }
        }

        void OnTurn()
        {
            if (--_LifeTime == 0)
                ExecuteMission();
        }

        void ExecuteMission()
        {
            switch (_Type)
            {
                //Добавление влияния
                case DipMissionType.InfAddition:
                    World.TheWorld.GetRegion(_RegID).AddInfluence(_AuthID, _Parameter);
                    break;
                //добавление благосостояния
                case DipMissionType.ProsperityAddition:
                    World.TheWorld.GetRegion(_RegID).AddProsperity(_Parameter);
                    break;
                //Кооперация (поднимается + $$$ в национальный фонд играбельной страны и + GDP не играбельной страны в течении определенного времени, например +100 через 50 ходов)
                case DipMissionType.Cooperation:
                    
                    break;
                //Постройка новой базы
                case DipMissionType.MilitaryBaseBuilding:
                    if (World.TheWorld.GetRegion(_RegID).MilitaryBaseID == 0)
                    {
                        World.TheWorld.BuildMillitaryBase(_RegID, _AuthID);
                    }
                    break;
                //Увеличение вместимости базы
                case DipMissionType.MilitaryBaseCapacityAddition:
                    if (World.TheWorld.GetRegion(_RegID).MilitaryBaseID != 0)
                    {
                        World.TheWorld.GetMilitaryBase(World.TheWorld.GetRegion(_RegID).MilitaryBaseID).AddCapacity(_Parameter);
                    }
                    break;
                case DipMissionType.AddSpy:
                    World.TheWorld.GetRegion(_RegID).AddSpy(_AuthID, _Parameter);
                    break;
                default:
                    break;
            }
        }
    }

    public enum DipMissionType
    {
        InfAddition,
        ProsperityAddition,
        Cooperation,
        MilitaryBaseBuilding,
        MilitaryBaseCapacityAddition,
        AddSpy
    }
}