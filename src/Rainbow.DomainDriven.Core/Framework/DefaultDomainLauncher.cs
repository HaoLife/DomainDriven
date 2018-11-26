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
        private IAggregateRootValidator _aggregateRootValidator;

        public DefaultDomainLauncher(
            IDatabaseInitializer databaseInitializer
            , IEventRebuildInitializer eventRebuildInitializer
            , IAggregateRootValidator aggregateRootValidator)
        {
            this._databaseInitializer = databaseInitializer;
            this._eventRebuildInitializer = eventRebuildInitializer;
            this._aggregateRootValidator = aggregateRootValidator;
        }

        public void Start()
        {
            _aggregateRootValidator.Validate();
            if (!_databaseInitializer.IsRun)
            {
                _databaseInitializer.Initialize();
            }

            _eventRebuildInitializer.Initialize();
        }

    }
}
