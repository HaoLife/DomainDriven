using System;
using Rainbow.DomainDriven.ConsoleApp.Domain;
using Rainbow.DomainDriven.ConsoleApp.Event.User;
using Rainbow.DomainDriven.Event;
using Rainbow.DomainDriven.Repository;

namespace Rainbow.DomainDriven.ConsoleApp.EventHandler.Cache
{
    public class UserHandler : IEventHandler<CreatedEvent>
    {
        private readonly IAggregateRootCommonQueryRepository _commonQueryRepository;

        //1.获取聚合跟仓库
        //2.获取存储仓库
        public UserHandler(
            IAggregateRootCommonQueryRepository commonQueryRepository)
        {
            this._commonQueryRepository = commonQueryRepository;
        }
        public void Handle(CreatedEvent evt)
        {
            var user = this._commonQueryRepository.Get<User>(evt.Id);
            Console.WriteLine($"执行变更缓存业务,更新用户:id:{user.Id} ,name:{user.Name} ,sex:{user.Sex}");
            throw new Exception("测试一个异常");
        }
    }
}