using Rainbow.DomainDriven.Message;
using System;

namespace Rainbow.DomainDriven.Command
{
    public static class CommandServiceExtensions
    {
        public static void Send<TCommand>(this ICommandService service, TCommand cmd) where TCommand : class
        {
            service.Publish<TCommand>(new DomainMessage()
            {
                Content = cmd,
                Head = new MessageHead(Guid.NewGuid().ToShort(), PriorityLevel.Normal, ConsistencyLevel.Lose)
            });
        }


        public static void SendQuick<TCommand>(this ICommandService service, TCommand cmd) where TCommand : class
        {
            service.Publish<TCommand>(new DomainMessage()
            {
                Content = cmd,
                Head = new MessageHead(Guid.NewGuid().ToShort(), PriorityLevel.Quick, ConsistencyLevel.Lose)
            });
        }


        public static void SendSlow<TCommand>(this ICommandService service, TCommand cmd) where TCommand : class
        {
            service.Publish<TCommand>(new DomainMessage()
            {
                Content = cmd,
                Head = new MessageHead(Guid.NewGuid().ToShort(), PriorityLevel.Slow, ConsistencyLevel.Lose)
            });
        }


        public static void Handle<TCommand>(this ICommandService service, TCommand cmd) where TCommand : class
        {
            service.Publish<TCommand>(new DomainMessage()
            {
                Content = cmd,
                Head = new MessageHead(Guid.NewGuid().ToShort(), Guid.NewGuid().ToShort(), PriorityLevel.Normal, ConsistencyLevel.Finally)
            });
        }


        public static void HandleQuick<TCommand>(this ICommandService service, TCommand cmd) where TCommand : class
        {
            service.Publish<TCommand>(new DomainMessage()
            {
                Content = cmd,
                Head = new MessageHead(Guid.NewGuid().ToShort(), Guid.NewGuid().ToShort(), PriorityLevel.Quick, ConsistencyLevel.Finally)
            });
        }


        public static void HandleSlow<TCommand>(this ICommandService service, TCommand cmd) where TCommand : class
        {
            service.Publish<TCommand>(new DomainMessage()
            {
                Content = cmd,
                Head = new MessageHead(Guid.NewGuid().ToShort(), Guid.NewGuid().ToShort(), PriorityLevel.Slow, ConsistencyLevel.Finally)
            });
        }
    }
}
