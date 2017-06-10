namespace Rainbow.DomainDriven.RingQueue.Utilities
{
    public class QueueName
    {
        public const string CommandQueue = "command";
        public const string CommandCacheConsumer = "command:cache";
        public const string CommandExecutorConsumer = "command:executor";
        public const string EventQueue = "event";
        public const string EventRecallConsumer = "event:recall";
        public const string EventExecutorConsumer = "event:executor";
    }
}