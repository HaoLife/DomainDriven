using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rainbow.DomainDriven.RingQueue.Framework
{
    public class RingConfigureOptions :  IConfigureOptions<RingOptions>
    {
        private readonly IConfiguration _configuration;

        public RingConfigureOptions(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void Configure(RingOptions options)
        {
            _configuration.Bind(options);
        }
    }
}
