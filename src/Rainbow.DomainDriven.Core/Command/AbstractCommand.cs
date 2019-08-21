using System;
using System.Collections.Generic;
using System.Text;

namespace Rainbow.DomainDriven.Command
{
    public abstract class AbstractCommand : ICommand
    {
        public AbstractCommand()
            :this(Guid.NewGuid())
        {

        }
        public AbstractCommand(Guid id, PriorityLevel priority = PriorityLevel.Normal, WaitLevel wait = WaitLevel.Handle)
        {
            this.Id = id;
            this.Priority = priority;
            this.Wait = wait;
        }

        public Guid Id { get; private set; }

        public PriorityLevel Priority { get; set; }

        public WaitLevel Wait { get; set; }
    }
}
