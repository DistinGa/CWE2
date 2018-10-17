using System;
using System.Collections;
using System.Collections.Generic;
using nsWorld;
using nsEventSystem;

namespace nsEmbassy
{
    public class DiplomaticMission
    {
        public int LifeTime;  //Продолжительность миссии

        public EventTypeClass EventType;
        public EventArgs EventArgs;

        public DiplomaticMission(int LifeTime, EventTypeClass EventType, EventArgs EventArgs)
        {
            this.LifeTime = LifeTime;
            this.EventType = EventType;
            this.EventArgs = EventArgs;
        }

        void ExecuteMission()
        {
            GameEventSystem.Instance.InvokeEvents(EventType, EventArgs);

            GameEventSystem.Instance.InvokeEvents(GameEventSystem.MyEventsTypes.SpyNetCompletesDipMission, EventArgs);

            //switch (_Type)
            //{
            //    //Добавление влияния
            //    case DipMissionType.InfAddition:
            //        World.TheWorld.GetRegion(_RegID).AddInfluence(_TargetID, _Parameter);
            //        break;
            //    //добавление благосостояния
            //    case DipMissionType.ProsperityAddition:
            //        World.TheWorld.GetRegion(_RegID).AddProsperity(_Parameter);
            //        break;
            //    //Кооперация (поднимается + $$$ в национальный фонд играбельной страны и + GDP не играбельной страны в течении определенного времени, например +100 через 50 ходов)
            //    case DipMissionType.Cooperation:
                    
            //        break;
            //    //Постройка новой базы
            //    case DipMissionType.MilitaryBaseBuilding:
            //        if (World.TheWorld.GetRegion(_RegID).MilitaryBaseID == 0)
            //        {
            //            nsMilitary.MilitaryManager.Instance.BuildMillitaryBase(_RegID, _TargetID);
            //        }
            //        break;
            //    //Увеличение вместимости базы
            //    case DipMissionType.MilitaryBaseCapacityAddition:
            //        if (World.TheWorld.GetRegion(_RegID).MilitaryBaseID != 0)
            //        {
            //            nsMilitary.MilitaryManager.Instance.GetMilitaryBase(World.TheWorld.GetRegion(_RegID).MilitaryBaseID).AddCapacity(_Parameter);
            //        }
            //        break;
            //    case DipMissionType.RiseParty:
            //         break;
            //    case DipMissionType.PushAct:
            //        break;
            //    case DipMissionType.RiseRadicals:
            //        break;
            //    case DipMissionType.Espionage:
            //        break;
            //    case DipMissionType.CounterEspionage:
            //        break;
            //    default:
            //        break;
            //}
        }
    }

    public enum DipMissionType
    {
        InfAddition,
        ProsperityAddition,
        Cooperation,
        MilitaryBaseBuilding,
        MilitaryBaseCapacityAddition,
        RiseParty,
        PushAct,
        RiseRadicals,
        Espionage,
        CounterEspionage
    }
}