using Microsoft.Extensions.Hosting;
using Rainbow.DomainDriven.Event;
using Rainbow.DomainDriven.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rainbow.DomainDriven.RingQueue.Framework
{
    public class RingBufferServerHostedService : IHostedService
    {
        private IDatabaseInitializer _databaseInitializer;
        private IRingBufferProcess _ringBufferProcess;
        private IEventRebuildInitializer _eventRebuildInitializer;
        private IEnumerable<IAggregateRootValidator> _aggregateRootValidators;

        public RingBufferServerHostedService(
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

        public Task StartAsync(CancellationToken cancellationToken)
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

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _ringBufferProcess.Stop();

            return Task.CompletedTask;
        }
    }
}
