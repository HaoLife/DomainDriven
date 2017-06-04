using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using MongoDB.Driver;

namespace Rainbow.DomainDriven.Mongo
{
    public class MongoDatabaseProvider : IMongoDatabaseProvider
    {
        private readonly IConfiguration _configuration;
        private IChangeToken _changeToken;
        private IDisposable _changeTokenRegistration;

        private IMongoDatabase _aggregateRootMongoDatabase;
        private IMongoDatabase _eventMongoDatabase;

        public MongoDatabaseProvider(IConfiguration configuration)
        {
            this._configuration = configuration;
            this.UseConfiguration(configuration);
        }

        private void UseConfiguration(IConfiguration configuration)
        {

            // unregister the previous configuration callback if there was one
            _changeTokenRegistration?.Dispose();

            if (configuration == null)
            {
                _changeToken = null;
                _changeTokenRegistration = null;
            }
            else
            {
                _changeToken = _configuration.GetReloadToken();
                _changeTokenRegistration = _changeToken?.RegisterChangeCallback(OnConfigurationReload, null);
            }

            Initialize();
        }

        private void OnConfigurationReload(object state)
        {

            _changeToken = _configuration.GetReloadToken();
            try
            {
                Initialize();
            }
            catch (Exception ex)
            { 
            }
            finally
            {
                _changeTokenRegistration = _changeToken.RegisterChangeCallback(OnConfigurationReload, null);
            }
        }

        private void Initialize()
        {
            LoadDatabase();
            LoadEventDatabase();
        }

        private void LoadDatabase()
        {
            var databaseName = this._configuration.GetValue<string>("Database");
            var connectionString = this._configuration.GetValue<string>("ConnectionString");

            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(databaseName);
            this._aggregateRootMongoDatabase = database;
        }

        private void LoadEventDatabase()
        {
            var databaseName = this._configuration.GetValue<string>("EventSourceDatabase")
                ?? this._configuration.GetValue<string>("Database"); ;
            var connectionString = this._configuration.GetValue<string>("EventSourceConnectionString")
                ?? this._configuration.GetValue<string>("ConnectionString"); ;

            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(databaseName);
            this._eventMongoDatabase = database;
        }

        private bool HasChanged => this._changeToken.HasChanged;


        public IMongoCollection<TEntity> GetCollection<TEntity>(string name)
        {
            return this._aggregateRootMongoDatabase.GetCollection<TEntity>(name);
        }

        public IMongoDatabase GetDatabase()
        {
            return this._aggregateRootMongoDatabase;
        }

        public IMongoCollection<TEntity> GetEventCollection<TEntity>(string name)
        {
            return this._eventMongoDatabase.GetCollection<TEntity>(name);
        }

        public IMongoDatabase GetEventDatabase()
        {
            return this._eventMongoDatabase;
        }
    }
}