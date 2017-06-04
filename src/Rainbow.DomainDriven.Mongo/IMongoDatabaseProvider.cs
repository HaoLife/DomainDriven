using MongoDB.Driver;

namespace Rainbow.DomainDriven.Mongo
{
    public interface IMongoDatabaseProvider
    {
        IMongoDatabase GetDatabase();
        IMongoCollection<TEntity> GetCollection<TEntity>(string name);
        IMongoDatabase GetEventDatabase();
        IMongoCollection<TEntity>  GetEventCollection<TEntity>(string name);
    }
}