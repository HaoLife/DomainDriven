using Rainbow.DomainDriven.Message;

namespace Rainbow.DomainDriven.Command
{
    public static class CommandServiceExtensions
    {

        public static void Send(this ICommandService service, ICommand cmd)
        {
            service.Publish(cmd, new MessageDescribe(Priority.Normal, Consistency.Lose));
        }


        public static void SendQuick(this ICommandService service, ICommand cmd)
        {
            service.Publish(cmd, new MessageDescribe(Priority.Quick, Consistency.Lose));
        }


        public static void SendSlow(this ICommandService service, ICommand cmd)
        {
            service.Publish(cmd, new MessageDescribe(Priority.Slow, Consistency.Lose));
        }


        public static void Handle(this ICommandService service, ICommand cmd)
        {
            service.Publish(cmd, new MessageDescribe(Priority.Normal, Consistency.Finally));
        }


        public static void HandleQuick(this ICommandService service, ICommand cmd)
        {
            service.Publish(cmd, new MessageDescribe(Priority.Quick, Consistency.Finally));
        }


        public static void HandleSlow(this ICommandService service, ICommand cmd)
        {
            service.Publish(cmd, new MessageDescribe(Priority.Slow, Consistency.Finally));
        }


        public static void Wait(this ICommandService service, ICommand cmd)
        {
            service.Publish(cmd, new MessageDescribe(Priority.Normal, Consistency.Strong));
        }


        public static void WaitQuick(this ICommandService service, ICommand cmd)
        {
            service.Publish(cmd, new MessageDescribe(Priority.Quick, Consistency.Strong));
        }


        public static void WaitSlow(this ICommandService service, ICommand cmd)
        {
            service.Publish(cmd, new MessageDescribe(Priority.Slow, Consistency.Strong));
        }
    }
}