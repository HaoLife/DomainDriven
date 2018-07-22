using System;
using System.Collections.Generic;
using System.Text;

namespace Rainbow.DomainDriven.Command
{
    public abstract class AbstractCommand : ICommand
    {
        public AbstractCommand()
        {
            Id = Guid.NewGuid();
        }
        public AbstractCommand(Guid id, int priority = 0)
        {
            this.Id = id;
            this.Priority = priority;
        }

        public Guid Id { get; private set; }

        public int Priority { get; private set; }
    }
}
