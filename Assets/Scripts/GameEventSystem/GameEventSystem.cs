using System;
using System.Collections;
using System.Collections.Generic;

namespace nsEventSystem
{
    //Класс-болванка используется для определения типа события. Класс - чтобы легче было искать в коде подписки и вызовы событий.
    public class EventTypeClass{}

    public sealed class GameEventSystem
    {
        //Класс используется для хранения объектных ключей словаря EventAggregator
        internal static class MyEventsTypes
        {
            //События без параметров
            public static readonly EventTypeClass TurnEvents = new EventTypeClass();
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

            //События с сылочными параметрами (не используются при сериализации)
        }

        //Класс используется для хранения объектных ключей словаря CalcEventAggregator
        internal static class MyCalcEventsTypes
        {

        }

        private static GameEventSystem _Instance;
        private static Dictionary<string, EventTypeClass> EventTypesDictionary = new Dictionary<string, EventTypeClass>();  //Возможность для задания событий строками (в целях сериализации)

        private static Dictionary<object, EventHandler> EventAggregator = new Dictionary<object, EventHandler>();
        private static Dictionary<object, List<Func<EventArgs, int>>> CalcEventAggregator = new Dictionary<object, List<Func<EventArgs, int>>>();

        public static GameEventSystem Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new GameEventSystem();

                return _Instance;
            }
        }

        public static void AddToEventTypesDictionary(string EventName, EventTypeClass EventType)
        {
            EventTypesDictionary[EventName] = EventType;
        }

        #region Common
        public void Subscribe(EventTypeClass evType, EventHandler evHandler)
        {
            if (!EventAggregator.ContainsKey(evType))
                EventAggregator.Add(evType, delegate { });

            EventAggregator[evType] += evHandler;
        }

         public void Subscribe(string evTypeString, EventHandler evHandler)
        {
            if (!EventTypesDictionary.ContainsKey(evTypeString))
                throw (new Exception("Subscribing event name is not exist in the EventTypesDictionary."));

            Subscribe(EventTypesDictionary[evTypeString], evHandler);
        }

       public void UnSubscribe(EventTypeClass evType, EventHandler evHandler)
        {
            if (!EventAggregator.ContainsKey(evType))
                throw (new Exception("Unsubscribing event is not exist."));

            EventAggregator[evType] -= evHandler;
        }

        public void UnSubscribe(string evTypeString, EventHandler evHandler)
        {
            if (!EventTypesDictionary.ContainsKey(evTypeString))
                throw (new Exception("Unsubscribing event name is not exist in the EventTypesDictionary."));

            UnSubscribe(EventTypesDictionary[evTypeString], evHandler);
        }

        public void InvokeEvents(EventTypeClass evType, object sender = null)
        {
            InvokeEvents(evType, EventArgs.Empty, sender);
        }

        public void InvokeEvents(string evTypeString, object sender = null)
        {
            if (!EventTypesDictionary.ContainsKey(evTypeString))
                throw (new Exception("Invoking event name is not exist in the EventTypesDictionary."));

            InvokeEvents(EventTypesDictionary[evTypeString], sender);
        }

        public void InvokeEvents(EventTypeClass evType, EventArgs e, object sender = null)
        {
            if (!EventAggregator.ContainsKey(evType))
                return;

            EventAggregator[evType](sender, e);
        }

        public void InvokeEvents(string evTypeString, EventArgs e)
        {
            if (!EventTypesDictionary.ContainsKey(evTypeString))
                throw (new Exception("Invoking event name is not exist in the EventTypesDictionary."));

            InvokeEvents(EventTypesDictionary[evTypeString], e);
        }

        //Calculated Events
        public void SubscribeCalc(EventTypeClass evType, Func<EventArgs, int> evHandler)
        {
            if (!CalcEventAggregator.ContainsKey(evType))
                CalcEventAggregator.Add(evType, new List<Func<EventArgs, int>>());

            CalcEventAggregator[evType].Add(evHandler);
        }

        public void UnSubscribeCalc(EventTypeClass evType, Func<EventArgs, int> evHandler)
        {
            if (!CalcEventAggregator.ContainsKey(evType))
                throw (new Exception("Unsubscribing event is not exist."));

            CalcEventAggregator[evType].Remove(evHandler);
        }

        public void InvokeEventsCalc(EventTypeClass evType)
        {
            InvokeEventsCalc(evType, EventArgs.Empty);
        }

        public int InvokeEventsCalc(EventTypeClass evType, EventArgs e)
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
        public void SpendingComplete(SpendsSubjects Subject, int UnitID, int Authority)
        {
            switch (Subject)
            {
                case SpendsSubjects.MilitaryUnit:
                    Instance.InvokeEvents(MyEventsTypes.ProduceNewMilitaryUnit, new ProduceNewUnits_EventArgs() {AuthID = Authority, UnitID = UnitID, Amount = 1 });
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

    public class GameEvent
    {
        public EventTypeClass EventType;
        public EventArgs EventArgs;
    }
}
