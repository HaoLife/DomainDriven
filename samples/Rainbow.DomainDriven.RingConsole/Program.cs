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
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Logging;

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

            serviceCollection.AddLogging(builder =>
                builder
                    .AddFile(opts => configuration.GetSection("FileLoggingOptions").Bind(opts))
                    .AddConsole()
            
            );



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

            var size = 10000;

            do
            {
                Task[] tasks = new Task[size];
                long seq = 0;
                System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                sw.Start();
                Console.WriteLine($"开始执行");
                for (var i = 0; i < size; i++, seq++)
                {
                    var createCommand = new CreateUserCommand()
                    {
                        UserId = Guid.NewGuid(),
                        Name = $"nihao 1-{seq}",
                        Sex = 1

                    };

                    var task = commandBus.Publish(createCommand);
                    tasks[i] = task;
                }
                Task.WaitAll(tasks);
                var errCount = tasks.Where(a => a.Exception != null).Count();
                Console.WriteLine($"执行：{size} 条 ms：{sw.ElapsedMilliseconds} 错误数：{errCount}");
                //} while (Console.ReadLine() != "c");
            } while (true);

            Console.WriteLine("Hello World!");
            Console.Read();
        }
    }
}
