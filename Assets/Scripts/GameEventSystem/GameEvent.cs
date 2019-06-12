using System;

namespace nsEventSystem
{
    public class GameEvent
    {
        public string EventType;
        public EventArgs EventArgs;

        public GameEvent(EventTypeClass EventType)
        {
            this.EventType = EventType.StringEventType;
        }

        public void Invoke()
        {
            GameEventSystem.InvokeEvents(EventType, EventArgs);
        }
    }
}
