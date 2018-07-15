using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using Rainbow.DomainDriven.Command;
using Rainbow.DomainDriven.ConsoleApp.Command;
using System.Text;
using Rainbow.DomainDriven.Domain;
using System.Data;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Rainbow.DomainDriven.Host;
using Rainbow.DomainDriven.ConsoleApp.Mapping;
using Rainbow.DomainDriven.ConsoleApp.Query;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Castle.DynamicProxy;
using System.Linq;
using Autofac.Builder;
using Autofac.Extras.DynamicProxy;
using Rainbow.DomainDriven.ConsoleApp.Interceptors;

namespace Rainbow.DomainDriven.ConsoleApp
{

    public class Program
    {
        public static void Main(string[] args)
        {
            AutoMapperInitializer.Init();
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            IServiceCollection serviceCollection = new ServiceCollection();

            serviceCollection.AddOptions();
            serviceCollection.Configure<SqliteOptions>(configuration.GetSection("Domain:Sqlite"));
            serviceCollection.AddTransient<IDbConnection>(a =>
                    {
                        var connectionString = a.GetService<IOptions<SqliteOptions>>().Value.ConnectionString;
                        connectionString = connectionString.Replace("${workspaceRoot}", Path.Combine(AppContext.BaseDirectory, "../../../"));

                        return new SqliteConnection(connectionString);
                    }

                );

            // serviceCollection
            //     //.UseLocalQueueDomain(configuration.GetSection("Domain:Local"))
            //     //.UseLocalMultiQueueDomain(configuration.GetSection("Domain:Local"))
            //     .UseDefaultDomain()
            //     //.UseDefaultService()
            //     //.UseCommandMapping<CommandMappingProvider>()
            //     .UseMongoAggregateRootRepository(configuration.GetSection("Domain:MongoDB"))
            //     //.UseEventSourcing()
            //     .Build()
            //     .Start();



            serviceCollection
                //.AddDomain()
                .AddLocalQueueDomain(configuration.GetSection("Domain:Local"))
                .AddDomainService()
                //.AddLocalQueueDomain(configuration.GetSection("Domain:Local"))
                //.AddLocalMultiQueueDomain(configuration.GetSection("Domain:Local"))
                .AddCommandMapping<CommandMappingProvider>()
                .AddMongo(configuration.GetSection("Domain:MongoDB"));

            serviceCollection.AddLogging();
            serviceCollection.AddSingleton<UserQuery>();


            var container = new ContainerBuilder();
            container.Populate(serviceCollection);

            //container.Populate

            container.RegisterType<LoggerInterceptor>();

            container.RegisterType<CommandService>()
                .As<ICommandService>()
                .EnableInterfaceInterceptors()
                .InterceptedBy(typeof(LoggerInterceptor));


            var provider = new AutofacServiceProvider(container.Build());

            //var provider = serviceCollection.BuildServiceProvider();

            var loggerFactory = provider.GetService<ILoggerFactory>();
            loggerFactory.AddDebug();
            loggerFactory.AddConsole(configuration.GetSection("Logging"));


            provider.UseDomain();
            
            var commandService = provider.GetRequiredService<ICommandService>();

            var logger = loggerFactory.CreateLogger<Program>();
            logger.LogDebug("开始调试");

            Guid id = new Guid("4c704243-55a7-408b-87c8-519193969c8b");
            try
            {
                commandService.Wait(new CreateUserCommand()
                {
                    Id = Guid.NewGuid(),
                    Name = "nihao 1-1",
                    Sex = 1
                });
            }
            catch (DomainException domainEx)
            {
                Console.WriteLine($"领域业务异常-异常代码：{domainEx.Code} 异常内容：{domainEx.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"未知异常-异常内容：{ex.Message}");
            }

            var query = provider.GetRequiredService<UserQuery>();
            var user = query.Query();

            Console.WriteLine("end");
            Console.ReadLine();

        }
    }


}
