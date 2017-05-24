using System;
using Rainbow.DomainDriven.ConsoleApp.Domain;
using Rainbow.DomainDriven.ConsoleApp.Event.UserEvent;
using Rainbow.DomainDriven.Event;
using Rainbow.DomainDriven.Repository;
using Dapper;
using System.Data;

namespace Rainbow.DomainDriven.ConsoleApp.EventHandler.Stroe
{
    public class UserHandler : IEventHandler<CreatedEvent>
    {
        private readonly IAggregateRootCommonQuery _commonQueryRepository;
        private readonly IDbConnection _connection;

        //1.获取聚合跟仓库
        //2.获取存储仓库
        public UserHandler(
            IAggregateRootCommonQuery commonQueryRepository,
            IDbConnection connection)
        {
            this._commonQueryRepository = commonQueryRepository;
            this._connection = connection;
        }
        public void Handle(CreatedEvent evt)
        {
            var user = this._commonQueryRepository.Get<User>(evt.Id);
            this._connection.Execute("insert into [User] ([Id],[Name],[Sex]) values(@Id,@Name,@Sex)", user);
        }
    }
}