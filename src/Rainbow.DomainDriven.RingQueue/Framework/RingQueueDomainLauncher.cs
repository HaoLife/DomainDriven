﻿using Rainbow.DomainDriven.Event;
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
        private IAggregateRootValidator _aggregateRootValidator;

        public RingQueueDomainLauncher(
            IDatabaseInitializer databaseInitializer
            , IRingBufferProcess ringBufferProcess
            , IEventRebuildInitializer eventRebuildInitializer
            , IAggregateRootValidator aggregateRootValidator)
        {
            this._databaseInitializer = databaseInitializer;
            this._ringBufferProcess = ringBufferProcess;
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
            if (!_ringBufferProcess.IsStart)
            {
                _ringBufferProcess.Start();
            }

            _eventRebuildInitializer.Initialize();
        }
    }
}
