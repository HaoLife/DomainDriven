# DomainDriven 领域驱动设计

这是一个领域驱动设计，我将打造多种模式去使用，默认模式、单机队列模式、分布式模式，同时他们可以用多种存储方式，
通过不同的类库去扩展存储，该框架使用.Net Core新的框架去编写，以适用跨平台的需求。


默认模式是为了去使用CQRS+DDD的一种最简单的方式，他不是为了优化性能而设计的模式，而是为了很好的规划代码，利用CQRS
分离写操作和读操作，写操作通过命令去变更领域模型，而领域模型把业务封装在模型中，这使业务高度集中，有时候可以通过领域
服务去添加一些业务，最终模型生成事件，到各个事件处理程序中，而事件处理程序可以写多个扩展。这个扩展可以做很多事情，比如
的领域模型可以存储在mongodb上，可是业务你可以放在mysql上，这时你生成了一个CreatedUserEvent的事件时,你可以Insert 一条
数据到mysql表中，也可以添加一个User的缓存，还可以去做创建用户的统计等一些信息，而EventHandler是可以多个扩展的，你可以合理
的去划分不同的业务，通过CreatedUserEvent这个事件，使得整个业务逻辑非常的赶紧（我最喜欢的其实就是这个功能）


单机队列模式我会重点去优化性能，借助Disruptor的环形队列模式，使command可以大量的堆积消息，然后通过单线程的模式+批量处理
+内存模式这种方式提升他的性能，而服务中设置的一致性就起到了作用，是弱一致、最终一致还是强一致就影响了他是否可进行丢失或
需要等待通知，在内存运算中，如果机器故障或者其他问题造成程序奔溃，那么在内存中处理的都将丢失，但只要是生产了事件并持久化
到DB中的，就可以通过事件溯源恢复数据，这也就是最终一致的结果，在使用是，可根据需求去设定是弱一致（可丢失）、最终一致（最终都会执行）
、强一致（等待所有都完成）。

### 这是一个默认模式的使用

    IConfiguration configuration = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .Build();
    
    IServiceCollection serviceCollection = new ServiceCollection();
    
    serviceCollection
        .UseDefaultDomain()
        .UseDefaultService()
        .UseTranMongoAggregateRootRepository(configuration.GetSection("Domain:MongoDB"))
        .Build()
        .Start();

    var provider = serviceCollection.BuildServiceProvider();
    var commandService = provider.GetRequiredService<ICommandService>();
    commandService.Handle(new CreateUserCommand()
    {
        Id = Guid.NewGuid(),
        Name = $"nihao-{id.ToShort()}",
        Sex = 1
    });


### 这是一个本地消息队列的使用

    IConfiguration configuration = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .Build();
    
    IServiceCollection serviceCollection = new ServiceCollection();
    
    serviceCollection
        .UseLocalQueueDomain(configuration.GetSection("Domain:Local"))
        .UseDefaultService()
        .UseTranMongoAggregateRootRepository(configuration.GetSection("Domain:MongoDB"))
        .Build()
        .Start();

    var provider = serviceCollection.BuildServiceProvider();
    var commandService = provider.GetRequiredService<ICommandService>();
    commandService.Handle(new CreateUserCommand()
    {
        Id = Guid.NewGuid(),
        Name = $"nihao-{id.ToShort()}",
        Sex = 1
    });


    appsettings.json

    "Local": {
        "CommandQueueSize": 1048576,
        "EventQueueSize": 1048576
    },
