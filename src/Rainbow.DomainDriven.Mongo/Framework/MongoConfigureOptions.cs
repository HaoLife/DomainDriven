using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rainbow.DomainDriven.Mongo.Framework
{
    public class MongoConfigureOptions : IConfigureOptions<MongoOptions>
    {
        private readonly IConfiguration _configuration;

        public MongoConfigureOptions(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void Configure(MongoOptions options)
        {
            _configuration.Bind(options);
        }
    }
}
