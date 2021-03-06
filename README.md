# DomainDriven 领域驱动设计

这是一个CQRS+DDD+EDA的框架，我将打造多种模式去使用，默认模式、单机队列模式、分布式模式，集群模式，同时他们可以用多种存储方式，
通过不同的类库去扩展存储，该框架使用.Net Core新的框架去编写，以适用跨平台的需求。


## 简介

### 目标
1.   默认模式：领域模型高度内聚业务逻辑，CQRS分离读写命令，EDA事件驱动去处理后续业务，让每个处理方法职责单一
2.   本地队列：优化性能，减少成本，降低一部分可用性
3.   分布式版：提高可用性，提高扩展能力，比本地模式降低了单台机器性能，整体性能提高，提高成本
4.   集群模式：暂未打算


### 参考资料
1.   领域驱动设计 [enode](https://github.com/tangxuehua/enode)
2.   环形队列 [Disruptor](https://github.com/disruptor-net/Disruptor-net)
3.   领域驱动设计 FCFramework



### 概述

#### 默认模式
默认模式是为了去使用CQRS+DDD+EDA的一种最简单的方式，他不是为了优化性能而设计的模式，而是为了很好的规划代码，利用CQRS
分离写操作和读操作，写操作通过命令去变更领域模型，而领域模型把业务封装在模型中，这使业务高度集中，有时候可以通过领域
服务去添加一些业务，最终模型生成事件，到各个事件处理程序中，而事件处理程序可以写多个扩展。这个扩展可以做很多事情，比如
的领域模型可以存储在mongodb上，可是业务你可以放在mysql上，这时你生成了一个CreatedUserEvent的事件时,你可以Insert 一条
数据到mysql表中，也可以添加一个User的缓存，还可以去做创建用户的统计等一些信息，而EventHandler是可以多个扩展的，你可以合理
的去划分不同的业务，通过CreatedUserEvent这个事件，使得整个业务逻辑非常的干净（我最喜欢的其实就是这个功能）

#### 本地队列
单机队列模式我会重点去优化性能，借助Disruptor的环形队列模式，使command可以大量的堆积消息，然后通过单线程的模式+批量处理
+内存模式这种方式提升他的性能，而服务中设置的一致性就起到了作用，是弱一致、最终一致还是强一致就影响了他是否可进行丢失或
需要等待通知，在内存运算中，如果机器故障或者其他问题造成程序奔溃，那么在内存中处理的都将丢失，但只要是生产了事件并持久化
到DB中的，就可以通过事件溯源恢复数据，这也就是最终一致的结果，在使用是，可根据需求去设定是弱一致（可丢失）、最终一致（最终都会执行）
、强一致（等待所有都完成）。

#### 分布式版
分布式版本是从本地队列模式中拆分出来，客户都（独立部署） 分布式换成、命令消费者（独立部署）、分布式缓存、事件消费者（独立部署）、分布式存储。从原来的2个程序，扩展成6N个程序。

#### 集群模式
集群模式是在分布式模式的基础上优化机器的利用率的思路，分布式模式是以无状态的方式去实现扩充的，而集群模式是以有状态的模式进行扩充需要解决的是command的hash路由、心跳、扩充后（或减少）后的路由变更和状态对象的迁移等一系列的问题，该模式会参考微软的oreans，现在比较多的问题还没有解决，所以暂时不打算去编码

## 说明和使用

### 这是一个默认模式的使用


![alt tag](https://raw.githubusercontent.com/HaoLife/DomainDriven/master/doc/image/synergy-default.jpg)

>     IConfiguration configuration = new ConfigurationBuilder()
>         .SetBasePath(Directory.GetCurrentDirectory())
>         .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
>         .Build();
>     
>     IServiceCollection serviceCollection = new ServiceCollection();
>     
>     serviceCollection
>         .UseDefaultDomain()
>         .UseDefaultService()
>         .UseTranMongoAggregateRootRepository(configuration.GetSection("Domain:MongoDB"))
>         .Build()
>         .Start();
>     
>     var provider = serviceCollection.BuildServiceProvider();
>     var commandService = provider.GetRequiredService<ICommandService>();
>     commandService.Handle(new CreateUserCommand()
>     {
>         Id = Guid.NewGuid(),
>         Name = $"nihao-{id.ToShort()}",
>         Sex = 1
>     });


### 这是一个本地消息队列的使用

![alt tag](https://raw.githubusercontent.com/HaoLife/DomainDriven/master/doc/image/synergy-localqueue.jpg)

>     IConfiguration configuration = new ConfigurationBuilder()
>         .SetBasePath(Directory.GetCurrentDirectory())
>         .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
>         .Build();
>     
>     IServiceCollection serviceCollection = new ServiceCollection();
>     
>     serviceCollection
>         .UseLocalQueueDomain(configuration.GetSection("Domain:Local"))
>         .UseDefaultService()
>         .UseTranMongoAggregateRootRepository(configuration.GetSection("Domain:MongoDB"))
>         .UseEventSourcing()   //事件回溯
>         .Build()
>         .Start();
>     
>     var provider = serviceCollection.BuildServiceProvider();
>     var commandService = provider.GetRequiredService<ICommandService>();
>     commandService.Handle(new CreateUserCommand()
>     {
>         Id = Guid.NewGuid(),
>         Name = $"nihao-{id.ToShort()}",
>         Sex = 1
>     });
>     
>     
>     appsettings.json
>     
>     "Local": {
>         "CommandQueueSize": 1048576,
>         "EventQueueSize": 1048576
>     },
