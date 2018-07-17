using System;
using System.Collections.Generic;
using System.Text;

namespace Rainbow.DomainDriven.Command
{
    public interface ICommandHandler<in TCommand>
    {
        void Handle(ICommandContext context, TCommand cmd);

    }
}
