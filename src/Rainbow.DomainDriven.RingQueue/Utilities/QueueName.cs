namespace Rainbow.DomainDriven.RingQueue.Utilities
{
    public class QueueName
    {
        public const string CommandQueue = "command";
        public const string CommandCacheConsumer = "cache";
        public const string CommandExecutorConsumer = "executor";
        public const string EventQueue = "event";
        public const string EventSnapshotConsumer = "snapshot";
        public const string EventExecutorConsumer = "executor";
    }
}