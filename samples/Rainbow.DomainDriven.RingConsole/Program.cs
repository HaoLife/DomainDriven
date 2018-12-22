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
using System.Threading;
using Rainbow.DomainDriven.Framework;

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
                    .AddConfiguration(configuration.GetSection("Logging"))
                    .AddFile(opts => configuration.GetSection("FileLoggingOptions").Bind(opts))
                    .AddConsole()

            );



            serviceCollection.AddDomain(builder =>
            {
                builder
                    .AddRing(configuration.GetSection("ring"))
                    .AddMongo(configuration.GetSection("mongo"))
                    .AddMixedMapping(mapbuilder =>
                    {
                        mapbuilder
                            .AddMapping<CommandMappingProvider>()
                            .AddAutoMapping();
                    })
                    .AddDomainService();
            });


            var container = new ContainerBuilder();
            container.Populate(serviceCollection);



            var provider = new AutofacServiceProvider(container.Build());

            var launcher = provider.GetRequiredService<IDomainLauncher>();
            launcher.Start();

            var commandBus = provider.GetRequiredService<ICommandBus>();
            var loggerFactory = provider.GetRequiredService<ILoggerFactory>();

            var logger = loggerFactory.CreateLogger<Program>();

            var size = 1;

            //var key = new Guid("{191cf76c-9bf4-46df-ab76-a52c52d4d47a}");
            do
            {
                Task[] tasks = new Task[size];
                long seq = 0;
                System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                sw.Start();
                logger.LogDebug("开始执行");
                for (var i = 0; i < size; i++, seq++)
                {
                    var temp = Guid.NewGuid();
                    var createCommand = new CreateUserCommand()
                    {
                        UserId = Guid.NewGuid(),
                        Name = $"nihao 1-{seq}",
                        Sex = 1,
                        UserId2 = temp,
                        UserId3 = temp,
                        Wait = WaitLevel.Snapshot
                    };

                    var task = commandBus.Publish(createCommand);

                    //var updateCommand = new ModifyUserNameCommand()
                    //{
                    //    UserId = key,
                    //    Name = "你好"
                    //};
                    //var task = commandBus.Publish(updateCommand);
                    tasks[i] = task;
                    //tasks[i] = Task.FromResult(true);
                }
                try
                {
                    Task.WaitAll(tasks);
                }
                catch (Exception ex)
                {

                }

                var errCount = tasks.Where(a => a.Exception != null).Count();
                logger.LogDebug($"执行：{size} 条 ms：{sw.ElapsedMilliseconds} 错误数：{errCount}");
                //Thread.Sleep(5000);
                //} while (Console.ReadLine() != "c");
            } while (true);

            Console.WriteLine("Hello World!");
            Console.Read();
        }
    }
}
