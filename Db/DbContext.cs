using DotnetMongoStarter.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DotnetMongoStarter.Db
{
    public class DbContext
    {
        public readonly IMongoCollection<User> Users;

        public DbContext(IOptions<DbSettings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            var database = client.GetDatabase(settings.Value.DatabaseName);

            var pingCommand = new BsonDocument("ping", 1);
            database.RunCommand<BsonDocument>(pingCommand);

            Users = database.GetCollection<User>(settings.Value.UserCollectionName);
        }
    }
}
