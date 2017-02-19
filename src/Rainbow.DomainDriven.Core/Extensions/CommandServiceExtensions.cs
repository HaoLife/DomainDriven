using Rainbow.DomainDriven.Command;
using Rainbow.DomainDriven.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rainbow.DomainDriven.Command
{
    public static class CommandServiceExtensions
    {
        public static void Send<TCommand>(this ICommandService service, TCommand cmd) where TCommand : class
        {
            service.Publish<TCommand>(new DomainMessage<TCommand>()
            {
                Content = cmd,
                Head = new MessageHead(Guid.NewGuid().ToShort(), PriorityLevel.Normal, ConsistencyLevel.Lose)
            });
        }


        public static void SendUrgent<TCommand>(this ICommandService service, TCommand cmd) where TCommand : class
        {
            service.Publish<TCommand>(new DomainMessage<TCommand>()
            {
                Content = cmd,
                Head = new MessageHead(Guid.NewGuid().ToShort(), PriorityLevel.Urgent, ConsistencyLevel.Lose)
            });
        }


        public static void SendSlow<TCommand>(this ICommandService service, TCommand cmd) where TCommand : class
        {
            service.Publish<TCommand>(new DomainMessage<TCommand>()
            {
                Content = cmd,
                Head = new MessageHead(Guid.NewGuid().ToShort(), PriorityLevel.Slow, ConsistencyLevel.Lose)
            });
        }


        public static void Handle<TCommand>(this ICommandService service, TCommand cmd) where TCommand : class
        {
            service.Publish<TCommand>(new DomainMessage<TCommand>()
            {
                Content = cmd,
                Head = new MessageHead(Guid.NewGuid().ToShort(), Guid.NewGuid().ToShort(), PriorityLevel.Normal, ConsistencyLevel.Lose)
            });
        }


        public static void HandleUrgent<TCommand>(this ICommandService service, TCommand cmd) where TCommand : class
        {
            service.Publish<TCommand>(new DomainMessage<TCommand>()
            {
                Content = cmd,
                Head = new MessageHead(Guid.NewGuid().ToShort(), Guid.NewGuid().ToShort(), PriorityLevel.Urgent, ConsistencyLevel.Lose)
            });
        }


        public static void HandleSlow<TCommand>(this ICommandService service, TCommand cmd) where TCommand : class
        {
            service.Publish<TCommand>(new DomainMessage<TCommand>()
            {
                Content = cmd,
                Head = new MessageHead(Guid.NewGuid().ToShort(), Guid.NewGuid().ToShort(), PriorityLevel.Slow, ConsistencyLevel.Lose)
            });
        }
    }
}
