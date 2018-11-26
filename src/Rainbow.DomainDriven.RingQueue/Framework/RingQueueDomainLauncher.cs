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

        public RingQueueDomainLauncher(
            IDatabaseInitializer databaseInitializer
            , IRingBufferProcess ringBufferProcess
            , IEventRebuildInitializer eventRebuildInitializer)
        {
            this._databaseInitializer = databaseInitializer;
            this._ringBufferProcess = ringBufferProcess;
            this._eventRebuildInitializer = eventRebuildInitializer;
        }

        public void Start()
        {
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
