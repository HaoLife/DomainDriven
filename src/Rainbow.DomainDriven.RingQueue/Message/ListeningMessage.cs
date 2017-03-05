namespace Rainbow.DomainDriven.RingQueue.Message
{
    public class ListeningMessage
    {
        public int Listening { get; set; }
        public NoticeMessage Message { get; set; }
    }
}