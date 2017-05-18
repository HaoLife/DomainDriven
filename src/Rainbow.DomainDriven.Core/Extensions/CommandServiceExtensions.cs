using Rainbow.DomainDriven.Message;

namespace Rainbow.DomainDriven.Command
{
    public static class CommandServiceExtensions
    {

        public static void Send(this ICommandService service, ICommand cmd)
        {
            service.Publish(new DomainMessage<ICommand>()
            {
                Content = cmd,
                Head = new MessageHead(Priority.Normal, Consistency.Lose)
            });
        }


        public static void SendQuick(this ICommandService service, ICommand cmd)
        {
            service.Publish(new DomainMessage<ICommand>()
            {
                Content = cmd,
                Head = new MessageHead(Priority.Quick, Consistency.Lose)
            });
        }


        public static void SendSlow(this ICommandService service, ICommand cmd)
        {
            service.Publish(new DomainMessage<ICommand>()
            {
                Content = cmd,
                Head = new MessageHead(Priority.Slow, Consistency.Lose)
            });
        }


        public static void Handle(this ICommandService service, ICommand cmd)
        {
            service.Publish(new DomainMessage<ICommand>()
            {
                Content = cmd,
                Head = new MessageHead(Priority.Normal, Consistency.Finally)
            });
        }


        public static void HandleQuick(this ICommandService service, ICommand cmd)
        {
            service.Publish(new DomainMessage<ICommand>()
            {
                Content = cmd,
                Head = new MessageHead(Priority.Quick, Consistency.Finally)
            });
        }


        public static void HandleSlow(this ICommandService service, ICommand cmd)
        {
            service.Publish(new DomainMessage<ICommand>()
            {
                Content = cmd,
                Head = new MessageHead(Priority.Slow, Consistency.Finally)
            });
        }
    }
}