using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Server.Models;

namespace Server.Services;

public abstract class MongoDBService
{
    public string CollectionName;
    protected IMongoDatabase Database;
    public MongoDBService(IOptions<MongoDBSettings> mongoDBSettings)
    {
        MongoClient client = new MongoClient(mongoDBSettings.Value.ConnectionURI);
        Database = client.GetDatabase(mongoDBSettings.Value.DatabaseName);
    }
   
}