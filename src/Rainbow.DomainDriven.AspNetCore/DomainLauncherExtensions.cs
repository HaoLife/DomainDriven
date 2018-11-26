using Microsoft.AspNetCore.Builder;
using System;
using Microsoft.Extensions.DependencyInjection;
using Rainbow.DomainDriven.Framework;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DomainLauncherExtensions
    {
        public static IApplicationBuilder UseDomain(this IApplicationBuilder builder)
        {
            var launcher = builder.ApplicationServices.GetRequiredService<IDomainLauncher>();
            launcher.Start();
            return builder;
        }
    }
}
