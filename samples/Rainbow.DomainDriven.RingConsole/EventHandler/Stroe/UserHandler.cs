using System;
using Rainbow.DomainDriven.RingConsole.Domain;
using Rainbow.DomainDriven.RingConsole.Event.UserEvent;
using Rainbow.DomainDriven.Event;
using System.Data;
using Rainbow.DomainDriven.Store;
using System.Linq;

namespace Rainbow.DomainDriven.RingConsole.EventHandler.Stroe
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

        }
    }
}