using Rainbow.DomainDriven.Event;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rainbow.DomainDriven.Framework
{
    public class DefaultDomainLauncher : IDomainLauncher
    {

        private IDatabaseInitializer _databaseInitializer;
        private IEventRebuildInitializer _eventRebuildInitializer;
        private IEnumerable<IAggregateRootValidator> _aggregateRootValidators;

        public DefaultDomainLauncher(
            IDatabaseInitializer databaseInitializer
            , IEventRebuildInitializer eventRebuildInitializer
            , IEnumerable<IAggregateRootValidator> aggregateRootValidators)
        {
            this._databaseInitializer = databaseInitializer;
            this._eventRebuildInitializer = eventRebuildInitializer;
            this._aggregateRootValidators = aggregateRootValidators;
        }

        public void Start()
        {
            foreach (var validator in _aggregateRootValidators)
            {
                validator.Validate();
            }
            if (!_databaseInitializer.IsRun)
            {
                _databaseInitializer.Initialize();
            }

            _eventRebuildInitializer.Initialize();
        }

    }
}
