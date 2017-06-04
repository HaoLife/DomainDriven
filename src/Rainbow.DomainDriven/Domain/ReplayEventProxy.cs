using System;
using System.Linq.Expressions;
using Rainbow.DomainDriven.Event;
using System.Reflection;
using System.Collections.Concurrent;

namespace Rainbow.DomainDriven.Domain
{
    public class ReplayEventProxy : IReplayEventProxy
    {
        private ConcurrentDictionary<Type, Delegate> _cacheInvokes = new ConcurrentDictionary<Type, Delegate>();

        public void Handle(IAggregateRoot root, IEvent evt)
        {
            var func = _cacheInvokes.GetOrAdd(evt.GetType(), GetDelegate);
            try
            {
                func.DynamicInvoke(root, evt);
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException;
            }
        }


        private Delegate GetDelegate(Type type)
        {
            var commandTypeExp = Expression.Parameter(type);
            var rootTypeExp = Expression.Parameter(typeof(IAggregateRoot));
            var instance = Expression.Constant(this);
            var methodType = this.GetType().GetMethod(nameof(HandleInvoke),
                BindingFlags.Instance | BindingFlags.NonPublic)
                .MakeGenericMethod(type);
            var callExp = Expression.Call(instance, methodType, rootTypeExp, commandTypeExp);
            return Expression.Lambda(callExp, rootTypeExp, commandTypeExp).Compile();
        }


        protected virtual void HandleInvoke<TEvent>(IAggregateRoot root, TEvent evt) where TEvent : class, IEvent
        {
            root.ReplayEvent<TEvent>(evt as TEvent);
        }
    }
}