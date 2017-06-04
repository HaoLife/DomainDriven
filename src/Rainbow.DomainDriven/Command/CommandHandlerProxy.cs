using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace Rainbow.DomainDriven.Command
{
    public class CommandHandlerProxy : ICommandHandlerProxy
    {
        private ConcurrentDictionary<Type, Delegate> _cacheInvokes = new ConcurrentDictionary<Type, Delegate>();

        private readonly ICommandHandlerSelector _commandHandlerSelector;
        private readonly ICommandHandlerActivator _commandHandlerActivator;
        public CommandHandlerProxy(
            ICommandHandlerSelector commandHandlerSelector,
            ICommandHandlerActivator commandHandlerActivator)
        {
            this._commandHandlerSelector = commandHandlerSelector;
            this._commandHandlerActivator = commandHandlerActivator;
        }


        private Delegate GetDelegate(Type type)
        {
            var commandTypeExp = Expression.Parameter(type);
            var contextTypeExp = Expression.Parameter(typeof(ICommandExecutorContext));
            var instance = Expression.Constant(this);
            var methodType = this.GetType().GetMethod(nameof(HandleInvoke),
                BindingFlags.Instance | BindingFlags.NonPublic)
                .MakeGenericMethod(type);
            var callExp = Expression.Call(instance, methodType, contextTypeExp, commandTypeExp);
            return Expression.Lambda(callExp, contextTypeExp, commandTypeExp).Compile();
        }

        public void Handle(ICommandExecutorContext context, ICommand command)
        {
            var func = _cacheInvokes.GetOrAdd(command.GetType(), GetDelegate);
            try
            {
                func.DynamicInvoke(context, command);
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException;
            }
        }


        protected virtual void HandleInvoke<TCommand>(ICommandExecutorContext context, TCommand command) where TCommand : ICommand 
        {
            var executorType = this._commandHandlerSelector.FindHandlerType<TCommand>();

            if (executorType == null) throw new NullReferenceException($"没有找到 {typeof(TCommand).Name} 的执行器类型");

            var executor = this._commandHandlerActivator.Create<TCommand>(executorType);
            if (executor == null) throw new NullReferenceException($"没有找到 {typeof(TCommand).Name} 的执行器类型");

            executor.Handler(context, command);
        }
    }
}