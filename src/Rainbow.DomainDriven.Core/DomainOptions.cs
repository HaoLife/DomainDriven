using System;
using System.Collections.Generic;

namespace Rainbow.DomainDriven
{
    public class DomainOptions
    {
        private readonly Dictionary<Type, IDomainOptionsExtension> _extensions;
        public DomainOptions()
        {
            _extensions = new Dictionary<Type, IDomainOptionsExtension>();
        }

        public IEnumerable<IDomainOptionsExtension> Extensions => _extensions.Values;

        public void Add(IDomainOptionsExtension item)
        {
            if (!this._extensions.ContainsKey(item.GetType()))
            {
                this._extensions.Add(item.GetType(), item);
            }
        }
    }
}