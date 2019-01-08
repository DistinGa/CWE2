using System;
using System.Collections;
using System.Collections.Generic;
using nsEventSystem;

namespace Combat
{
    public class Combat
    {
        Combat_DS combatData;

        public Combat()
        {
            GameEventSystem.Subscribe(GameEventSystem.MyEventsTypes.TurnActions, OnTurn);
        }

        ~Combat()
        {
            GameEventSystem.UnSubscribe(GameEventSystem.MyEventsTypes.TurnActions, OnTurn);
        }

        void OnTurn(object sender, EventArgs e)
        {

        }
    }

    public class Combat_DS
    {

    }
}
