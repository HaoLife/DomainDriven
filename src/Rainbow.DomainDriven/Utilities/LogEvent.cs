using Microsoft.Extensions.Logging;

namespace Rainbow.DomainDriven.Core.Utilities
{
    public class LogEvent
    {
        public static EventId Frame = new EventId(0, "框架");
        public static EventId Domain = new EventId(1, "领域");
        public static EventId CommandHandle = new EventId(2, "命令处理");
        public static EventId EventHandle = new EventId(4, "事件处理");
    }
}