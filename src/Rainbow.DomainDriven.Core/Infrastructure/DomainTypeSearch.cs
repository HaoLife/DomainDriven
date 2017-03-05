using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Rainbow.DomainDriven.Domain;
using Rainbow.DomainDriven.Event;

namespace Rainbow.DomainDriven.Infrastructure
{
    public class DomainTypeSearch : IDomainTypeSearch
    {
        private readonly Dictionary<string, Type> _cacheType;
        private readonly List<Func<Type, bool>> _checks;
        public DomainTypeSearch()
        {
            this._cacheType = new Dictionary<string, Type>();
            this._checks = new List<Func<Type, bool>>()
            {
                IsCommand,
                IsAggregateRoot,
                IsEvent
            };
        }

        public Type GetType(string name)
        {
            Type type;
            this._cacheType.TryGetValue(name, out type);
            return type;
        }

        protected virtual bool IsCommand(Type type)
        {
            return type.Name.EndsWith("Command");
        }

        protected virtual bool IsAggregateRoot(Type type)
        {
            if (!typeof(IAggregateRoot).IsAssignableFrom(type)) return false;
            var typeinfo = type.GetTypeInfo();
            return typeinfo.IsClass && !typeinfo.IsAbstract;
        }

        protected virtual bool IsEvent(Type type)
        {
            if (!typeof(IEvent).IsAssignableFrom(type)) return false;
            var typeinfo = type.GetTypeInfo();
            return typeinfo.IsClass && !typeinfo.IsAbstract;
        }

        public void Initialize(IEnumerable<Assembly> assemblys)
        {
            this.Register(assemblys.SelectMany(p => p.GetTypes()));
        }


        private void Register(Type type)
        {
            if (!this._checks.Any(check => check(type))) return;

            this._cacheType.Add(type.Name, type);
        }

        private void Register(IEnumerable<Type> types)
        {
            foreach (var type in types)
            {
                this.Register(type);
            }
        }

    }
}