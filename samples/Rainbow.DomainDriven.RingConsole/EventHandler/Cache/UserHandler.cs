using System;
using Rainbow.DomainDriven.RingConsole.Domain;
using Rainbow.DomainDriven.RingConsole.Event.UserEvent;
using Rainbow.DomainDriven.Event;
using Rainbow.DomainDriven.Store;
using System.Linq;

namespace Rainbow.DomainDriven.RingConsole.EventHandler.Cache
{
    public class UserHandler : IEventHandler<CreatedEvent>
    {
        private readonly ISnapshootQuery<User> _snapshootQuery;

        //1.获取聚合跟仓库
        //2.获取存储仓库
        public UserHandler(
            ISnapshootQueryFactory snapshootQueryFactory)
        {
            _snapshootQuery = snapshootQueryFactory.Create<User>();
        }
        public void Handle(CreatedEvent evt)
        {
            //var user = this._snapshootQuery.FirstOrDefault(a => a.Id == evt.AggregateRootId);
            //Console.WriteLine($"执行变更缓存业务,更新用户:id:{user.Id} ,name:{user.Name} ,sex:{user.Sex}");
            //throw new Exception("测试一个异常");
        }
    }
}