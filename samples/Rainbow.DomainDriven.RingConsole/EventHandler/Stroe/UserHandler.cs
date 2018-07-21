using System;
using Rainbow.DomainDriven.RingConsole.Domain;
using Rainbow.DomainDriven.RingConsole.Event.UserEvent;
using Rainbow.DomainDriven.Event;
using Dapper;
using System.Data;
using Rainbow.DomainDriven.Store;
using System.Linq;

namespace Rainbow.DomainDriven.RingConsole.EventHandler.Stroe
{
    public class UserHandler : IEventHandler<CreatedEvent>
    {
        private readonly ISnapshootQuery<User> _snapshootQuery;
        private readonly IDbConnection _connection;

        //1.获取聚合跟仓库
        //2.获取存储仓库
        public UserHandler(
            ISnapshootQueryFactory snapshootQueryFactory
            , IDbConnection connection)
        {
            _snapshootQuery = snapshootQueryFactory.Create<User>();
            this._connection = connection;
        }
        public void Handle(CreatedEvent evt)
        {
            var user = this._snapshootQuery.FirstOrDefault(a => a.Id == evt.AggregateRootId);
            this._connection.Execute("insert into [User] ([Id],[Name],[Sex]) values(@Id,@Name,@Sex)", user);
        }
    }
}