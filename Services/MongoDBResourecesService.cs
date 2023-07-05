using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Server.Models;
using SharedLibrary;

namespace Server.Services;

public class MongoDBResourecesService : MongoDBService
{
    public readonly IMongoCollection<Resources> _resourcesCollection;

    // public MongoDBAccountService(IOptions<MongoDBSettings> mongoDBSettings)
    // {
    //     MongoClient client = new MongoClient(mongoDBSettings.Value.ConnectionURI);
    //     IMongoDatabase database = client.GetDatabase(mongoDBSettings.Value.DatabaseName);
   
    // }
    public MongoDBResourecesService(IOptions<MongoDBSettings> mongoDBSettings) : base(mongoDBSettings)
    {
        CollectionName = "Resources";
        _resourcesCollection = Database.GetCollection<Resources>(CollectionName);
    }
    public async Task<List<Resources>> GetAsync() =>
        await _resourcesCollection.Find(_ => true).ToListAsync();
    
    public async Task<Resources> GetAsync(string userId) =>
        await _resourcesCollection.Find(user=>user.UserId.Equals(userId)).FirstOrDefaultAsync();
    
    public async Task CreateAsync(Resources resource) =>
        await _resourcesCollection.InsertOneAsync(resource);
    
    public async Task UpdateAsync(string resourceId,Resources resources) =>
        await _resourcesCollection.ReplaceOneAsync(user=>user.UserId.Equals(resourceId),resources);
    public async Task DeleteAsync(string userId) =>
        await _resourcesCollection.DeleteOneAsync(user => user.UserId.Equals(userId));
}