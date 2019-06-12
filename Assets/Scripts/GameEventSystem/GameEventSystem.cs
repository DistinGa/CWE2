﻿using System;
using System.Collections;
using System.Collections.Generic;

namespace nsEventSystem
{
    //Класс-болванка используется для определения типа события. Класс - чтобы легче было искать в коде подписки и вызовы событий.
    public class EventTypeClass
    {
        public string StringEventType { get; set; }

        //public EventTypeClass(string strEventType)
        //{

        //}
    }

    public static class GameEventSystem
    {
        //Класс используется для хранения объектных ключей словаря EventAggregator
        internal static class MyEventsTypes
        {
            private static Dictionary<string, EventTypeClass> EventTypesDictionary = new Dictionary<string, EventTypeClass>();  //Возможность для задания событий строками (в целях сериализации)

            public static void AddToEventTypesDictionary(string EventName, EventTypeClass EventType)
            {
                EventTypesDictionary[EventName] = EventType;
            }

            public static EventTypeClass GetFromEventTypesDictionary(string EventName)
            {
                if (!EventTypesDictionary.ContainsKey(EventName))
                    throw (new Exception("Event name is not exist in the EventTypesDictionary."));

                return EventTypesDictionary[EventName];
            }

            //События без параметров
            public static readonly EventTypeClass TurnActions = new EventTypeClass();   //Действия хода
            public static readonly EventTypeClass TurnResults = new EventTypeClass();   //Проверка результатов хода (вызывается после TurnActions)
            public static readonly EventTypeClass EndMonthEvents = new EventTypeClass();
            public static readonly EventTypeClass NewMonthEvents = new EventTypeClass();
            public static readonly EventTypeClass EndYearEvents = new EventTypeClass();
            public static readonly EventTypeClass NewYearEvents = new EventTypeClass();
            //События без параметров, где важен sender
            public static readonly EventTypeClass SpyNetCompletesDipMission = new EventTypeClass(); //Дипломатическая миссия выполнена

            //События с параметрами
            public static readonly EventTypeClass DevelopNewMilitaryUnit = new EventTypeClass(); //Разработка нового класса юнитов
            public static readonly EventTypeClass ProduceNewMilitaryUnit = new EventTypeClass(); //Постройка нового юнита (ProduceNewUnits_EventArgs)
            public static readonly EventTypeClass AbortPartyLawInRegion = new EventTypeClass(); //Отмена принятия закона оппозицией или игроком.
            public static readonly EventTypeClass EndOfCombat = new EventTypeClass(); //Окончание боя.
            public static readonly EventTypeClass AttackBattleAction = new EventTypeClass();    //Атака в бою.
            public static readonly EventTypeClass AddUnitsToWar = new EventTypeClass();    //Добавление юнитов в пул войны.
            public static readonly EventTypeClass AddNews = new EventTypeClass();    //Добавление новости.
            public static readonly EventTypeClass ChangeAuthority = new EventTypeClass();    //Смена политического режима.
            public static readonly EventTypeClass AddProsperity = new EventTypeClass();    //Изменение уровня благосостояния.

            //События с сылочными параметрами (не используются при сериализации)
        }

        //Класс используется для хранения объектных ключей словаря CalcEventAggregator (используются для расчётов, например скидок и бонусов)
        internal static class MyCalcEventsTypes
        {

        }

        private static Dictionary<object, EventHandler> EventAggregator = new Dictionary<object, EventHandler>();
        private static Dictionary<object, List<Func<EventArgs, int>>> CalcEventAggregator = new Dictionary<object, List<Func<EventArgs, int>>>();

        #region Common
        public static void Subscribe(EventTypeClass evType, EventHandler evHandler)
        {
            if (!EventAggregator.ContainsKey(evType))
                EventAggregator.Add(evType, delegate { });

            EventAggregator[evType] += evHandler;
        }

        public static void Subscribe(string evTypeString, EventHandler evHandler)
        {
            Subscribe(MyEventsTypes.GetFromEventTypesDictionary(evTypeString), evHandler);
        }

       public static void UnSubscribe(EventTypeClass evType, EventHandler evHandler)
        {
            if (!EventAggregator.ContainsKey(evType))
                throw (new Exception("Unsubscribing event is not exist."));

            EventAggregator[evType] -= evHandler;
        }

        public static void UnSubscribe(string evTypeString, EventHandler evHandler)
        {
            UnSubscribe(MyEventsTypes.GetFromEventTypesDictionary(evTypeString), evHandler);
        }

        public static void InvokeEvents(EventTypeClass evType, object sender = null)
        {
            InvokeEvents(evType, EventArgs.Empty, sender);
        }

        public static void InvokeEvents(string evTypeString, object sender = null)
        {
            InvokeEvents(MyEventsTypes.GetFromEventTypesDictionary(evTypeString), sender);
        }

        public static void InvokeEvents(EventTypeClass evType, EventArgs e, object sender = null)
        {
            if (!EventAggregator.ContainsKey(evType))
                return;

            EventAggregator[evType](sender, e);
        }

        public static void InvokeEvents(string evTypeString, EventArgs e)
        {
            InvokeEvents(MyEventsTypes.GetFromEventTypesDictionary(evTypeString), e);
        }

        //Calculated Events
        public static void SubscribeCalc(EventTypeClass evType, Func<EventArgs, int> evHandler)
        {
            if (!CalcEventAggregator.ContainsKey(evType))
                CalcEventAggregator.Add(evType, new List<Func<EventArgs, int>>());

            CalcEventAggregator[evType].Add(evHandler);
        }

        public static void UnSubscribeCalc(EventTypeClass evType, Func<EventArgs, int> evHandler)
        {
            if (!CalcEventAggregator.ContainsKey(evType))
                throw (new Exception("Unsubscribing event is not exist."));

            CalcEventAggregator[evType].Remove(evHandler);
        }

        public static void InvokeEventsCalc(EventTypeClass evType)
        {
            InvokeEventsCalc(evType, EventArgs.Empty);
        }

        public static int InvokeEventsCalc(EventTypeClass evType, EventArgs e)
        {
            if (!CalcEventAggregator.ContainsKey(evType))
                return 0;

            int res = 0;

            foreach (var item in CalcEventAggregator[evType])
            {
                res += item(e);
            }

            return res;
        }
        #endregion

        #region Budget
        public static void SpendingComplete(SpendsSubjects Subject, int UnitID, int Authority)
        {
            switch (Subject)
            {
                case SpendsSubjects.MilitaryUnit:
                    InvokeEvents(MyEventsTypes.ProduceNewMilitaryUnit, new ProduceNewUnits_EventArgs() { RegID = nsWorld.World.TheWorld.GetRegionController(Authority).HomelandID, UnitID = UnitID, Amount = 1 });
                    break;
                case SpendsSubjects.CosmoUnit:
                    break;
                case SpendsSubjects.Upgrade:
                    break;
                case SpendsSubjects.TechMilitary:
                    break;
                case SpendsSubjects.TechProduction:
                    break;
                default:
                    break;
            }
        }
        #endregion

    }

}
