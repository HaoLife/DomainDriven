using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Rainbow.DomainDriven.Domain;

namespace Rainbow.DomainDriven.Event
{
    public class EventRebuildHandler : IEventRebuildHandler
    {

        private readonly ConcurrentDictionary<Type, Action<IAggregateRoot, IEvent>> _cache = new ConcurrentDictionary<Type, Action<IAggregateRoot, IEvent>>();

        private static readonly MethodInfo _handleMethod = typeof(EventRebuildHandler).GetMethod(nameof(HandleInvoke), BindingFlags.Instance | BindingFlags.NonPublic);


        public void Handle(IAggregateRoot root, IEvent evt)
        {
            var call = _cache.GetOrAdd(
                key: evt.GetType(),
                valueFactory: (type) =>
                {
                    var getHandleMethod = _handleMethod.MakeGenericMethod(type);
                    var instance = Expression.Constant(this);
                    var parameter = Expression.Parameter(typeof(IAggregateRoot), "root");
                    var parameter2 = Expression.Parameter(typeof(IEvent), "evt");
                    var expression =
                        Expression.Lambda<Action<IAggregateRoot, IEvent>>(
                            Expression.Call(instance, getHandleMethod, parameter, parameter2),
                            parameter, parameter2);
                    return expression.Compile();
                });

            call(root, evt);
        }


        private void HandleInvoke<TEvent>(IAggregateRoot root, IEvent evt) where TEvent : class, IEvent
        {
            root.ReplayEvent<TEvent>(evt as TEvent);
        }
    }
}
