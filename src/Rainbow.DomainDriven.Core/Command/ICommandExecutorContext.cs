﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rainbow.DomainDriven.Command
{
    public interface ICommandExecutorContext: ITrackerContext, ICommandContext
    {
    }
}
