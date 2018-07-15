using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;

namespace Rainbow.DomainDriven.RingConsole
{
    class Program
    {
        static void Main(string[] args)
        {

            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            IServiceCollection serviceCollection = new ServiceCollection();

            serviceCollection.AddOptions();



            serviceCollection.AddDomain(builder =>
            {
            });


            var container = new ContainerBuilder();
            container.Populate(serviceCollection);



            var provider = new AutofacServiceProvider(container.Build());



            Console.WriteLine("Hello World!");
        }
    }
}
