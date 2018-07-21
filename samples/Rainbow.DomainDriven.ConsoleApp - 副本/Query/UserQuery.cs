using Rainbow.DomainDriven.ConsoleApp.Domain;
using Rainbow.DomainDriven.ConsoleApp.Info;
using Rainbow.DomainDriven.Repository;
using System.Linq;
using System;

namespace Rainbow.DomainDriven.ConsoleApp.Query
{
    public class UserQuery
    {
        private readonly IAggregateRootExprssionQueryOfT<User> _query;
        public UserQuery(IAggregateRootExprssionQueryOfT<User> query)
        {
            this._query = query;
        }


        public UserInfo Query()
        {
            return this._query.First().Map<UserInfo>();
        }
    }
}