using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;
using System.Linq;
using System.Collections.Generic;
using Rainbow.DomainDriven.Mongo.Repository;
using Rainbow.DomainDriven.Infrastructure;

namespace Rainbow.DomainDriven.Mongo.Infrastructure
{
    public class MongoInitializeExtension : IDomainInitializeExtension
    {
        public void ApplyServices(IServiceCollection services)
        {
        }
    }
}