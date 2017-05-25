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
using Rainbow.DomainDriven.ConsoleApp.Config;
using Microsoft.Extensions.Logging;

namespace Rainbow.DomainDriven.ConsoleApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            IServiceCollection serviceCollection = new ServiceCollection();

            serviceCollection
                .UseLocalQueueDomain(configuration.GetSection("Domain:Local"))
                //.UseLocalMultiQueueDomain(configuration.GetSection("Domain:Local"))
                //.UseDefaultDomain()
                //.UseDefaultService()
                //.UseCommandMapping<CommandMappingProvider>()
                .UseMongoAggregateRootRepository(configuration.GetSection("Domain:MongoDB"))
                //.UseEventSourcing()
                .Build()
                .Start();

            serviceCollection.Configure<SqliteOptions>(configuration.GetSection("Domain:Sqlite"));
            serviceCollection.AddTransient<IDbConnection>(a =>
                    new SqliteConnection(a.GetService<IOptions<SqliteOptions>>().Value.ConnectionString.Replace("${workspaceRoot}", Path.Combine(AppContext.BaseDirectory, "..\\..\\..\\")))
                );


            var provider = serviceCollection.BuildServiceProvider();

            var loggerFactory = provider.GetService<ILoggerFactory>();
            loggerFactory.AddDebug();
            loggerFactory.AddConsole(configuration.GetSection("Logging"));

            var commandService = provider.GetRequiredService<ICommandService>();
            //commandService.Handle(new CreateUserCommand() { Id = Guid.NewGuid(), Name = "nihao 1", Sex = 1 });


            Guid id = new Guid("4c704243-55a7-408b-87c8-519193969c8b");
            // commandService.Send(new CreateUserCommand()
            // {
            //     Id = id,
            //     Name = "nihao 1",
            //     Sex = 1
            // });

            try
            {
                commandService.Handle(new CreateUserCommand()
                {
                    Id = Guid.NewGuid(),
                    Name = "nihao 1-1",
                    Sex = 1
                });
                // commandService.Handle(new ModifyUserNameCommand()
                // {
                //     UserId = id,
                //     Name = "11"
                // });
            }
            catch (DomainException domainEx)
            {
                Console.WriteLine($"领域业务异常-异常代码：{domainEx.Code} 异常内容：{domainEx.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"未知异常-异常内容：{ex.Message}");
            }

            // for (int i = 0; i < 3; i++)
            // {
            //     Task.Factory.StartNew((a) =>
            //     {
            //         try
            //         {
            //             commandService.Handle(new CreateUserCommand()
            //             {
            //                 Id = Guid.NewGuid(),
            //                 Name = $"nihao-{id.ToShort()}",
            //                 Sex = 1
            //             });
            //         }
            //         catch (Exception e)
            //         {
            //             Console.WriteLine($"执行并非线程错误：{a} - 错误原因：{e.Message}");
            //         }

            //     }, i);
            // }

            Console.WriteLine("end");
            Console.ReadLine();

        }
    }


}
