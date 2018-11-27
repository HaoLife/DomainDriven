using Rainbow.DomainDriven.Event;
using Rainbow.DomainDriven.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rainbow.DomainDriven.RingQueue.Framework
{
    public class RingQueueDomainLauncher : IDomainLauncher
    {
        private IDatabaseInitializer _databaseInitializer;
        private IRingBufferProcess _ringBufferProcess;
        private IEventRebuildInitializer _eventRebuildInitializer;
        private IEnumerable<IAggregateRootValidator> _aggregateRootValidators;

        public RingQueueDomainLauncher(
            IDatabaseInitializer databaseInitializer
            , IRingBufferProcess ringBufferProcess
            , IEventRebuildInitializer eventRebuildInitializer
            , IEnumerable<IAggregateRootValidator> aggregateRootValidators)
        {
            this._databaseInitializer = databaseInitializer;
            this._ringBufferProcess = ringBufferProcess;
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
            if (!_ringBufferProcess.IsStart)
            {
                _ringBufferProcess.Start();
            }

            _eventRebuildInitializer.Initialize();
        }
    }
}
