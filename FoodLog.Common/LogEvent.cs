using System.Collections.Generic;

namespace FoodLog.Common
{
    public class LogEvent
    {
        public LogEvent(string eventName, Dictionary<string, string> data)
        {
            EventName = eventName;
            Data = data;
        }

        public string EventName { get; }
        public Dictionary<string, string> Data { get; }
    }
}