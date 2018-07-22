using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rainbow.DomainDriven.Command;
using Rainbow.DomainDriven.Event;
using Rainbow.DomainDriven.RingConsole.Command;
using Rainbow.DomainDriven.RingConsole.Mapping;
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
                builder
                    .AddRing(configuration.GetSection("ring"))
                    .AddMongo(configuration.GetSection("mongo"))
                    .AddMapping<CommandMappingProvider>()
                    .AddDomainService();
            });


            var container = new ContainerBuilder();
            container.Populate(serviceCollection);



            var provider = new AutofacServiceProvider(container.Build());

            var eventRebuildInitializer = provider.GetRequiredService<IEventRebuildInitializer>();
            eventRebuildInitializer.Initialize();

            var commandBus = provider.GetRequiredService<ICommandBus>();

            var createCommand = new CreateUserCommand()
            {
                UserId = Guid.NewGuid(),
                Name = "nihao 1-1",
                Sex = 1

            };

            var task = commandBus.Publish(createCommand);
            task.Wait();

            Console.WriteLine("Hello World!");
            Console.Read();
        }
    }
}
