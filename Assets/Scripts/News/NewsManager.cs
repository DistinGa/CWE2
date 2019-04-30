using System;
using System.Collections;
using System.Collections.Generic;
using nsEventSystem;

namespace nsNews
{
    public class NewsManager
    {
        NewsManager_DS newsLineData;

        public NewsManager()
        {
            newsLineData = new NewsManager_DS();

            GameEventSystem.Subscribe(GameEventSystem.MyEventsTypes.AddNews, AddNews);
        }

        ~NewsManager()
        {
            GameEventSystem.UnSubscribe(GameEventSystem.MyEventsTypes.AddNews, AddNews);
        }

        void AddNews(object sender, EventArgs e)
        {
            var _args = e as AddNews_EventArgs;

            newsLineData.NewsLine.Add(new News(_args.RegionID, _args.InitTurn, _args.TextID));
        }
    }

    public class NewsManager_DS : ISavable
    {
        public List<News> NewsLine = new List<News>();
    }
}
