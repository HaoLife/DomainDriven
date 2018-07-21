using Rainbow.DomainDriven.RingConsole.Domain;
using Rainbow.DomainDriven.Domain;
using System.Linq;
// using Rainbow.DomainDriven.Cache;

namespace Rainbow.DomainDriven.RingConsole.DomainService
{
    // public class UserService : IDomainService
    // {
    //     private readonly IAggregateRootQueryable<User> _queryRepo;
    //     private readonly IAggregateRootIndexCache _indexCache;
    //     public UserService(IAggregateRootQueryable<User> queryRepo, IAggregateRootIndexCache indexCache)
    //     {
    //         this._queryRepo = queryRepo;
    //         this._indexCache = indexCache;
    //     }

    //     public void RegisterUser(User user)
    //     {
    //         var isExists = _queryRepo.Where(a => a.Name == user.Name).Any();
    //         if (isExists)
    //             throw new DomainException(DomainCode.IndexCacheExists.GetHashCode(), $"用户名：{user.Name}已存在");

    //         var indexKey = user.GetIndex(user.Name);
    //         this._indexCache.Add(user, indexKey);
    //     }

    // }
}