using GameServer.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Server.Models;

namespace GameServer.Infrastructure;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;
    public MongoDbContext(IOptions<MongoDBSettings> mongoDbSettings)
    {
        var client = new MongoClient(mongoDbSettings.Value.ConnectionURI);
        _database = client.GetDatabase(mongoDbSettings.Value.DatabaseName);
    }
    public IMongoCollection<User> Users => _database.GetCollection<User>("Accounts");
    public IMongoCollection<Resource> Resources => _database.GetCollection<Resource>("Resources");
}