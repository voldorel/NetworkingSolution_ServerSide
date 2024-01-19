using GameServer.Infrastructure;
using GameServer.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using SharedLibrary;

namespace GameServer.Repositories;

public class ResourcesRepository
{
    private readonly IMongoCollection<Resource> _resourcesCollection;
    public ResourcesRepository(MongoDbContext dbContext) 
    {
        _resourcesCollection = dbContext.Resources;
        var indexKeysDefinition = Builders<Resource>.IndexKeys.Ascending(resource => resource.UserId);
        var tokenIndexModel = new CreateIndexModel<Resource>(indexKeysDefinition);
        _resourcesCollection.Indexes.CreateOne(tokenIndexModel);
    }
    
    public async Task<List<Resource>> GetAsync() =>
        await _resourcesCollection.Find(_ => true).ToListAsync();
    
    public async Task<Resource> GetAsync(string userId) =>
        await _resourcesCollection.Find(user=>user.UserId.Equals(userId)).FirstOrDefaultAsync();
    
    public async Task CreateAsync(Resource resource) =>
        await _resourcesCollection.InsertOneAsync(resource);
    
    public async Task UpdateAsync(ObjectId resourceId,Resource resource) =>
        await _resourcesCollection.ReplaceOneAsync(user=>user.UserId.Equals(resourceId),resource);
    public async Task DeleteAsync(string userId) =>
        await _resourcesCollection.DeleteOneAsync(user => user.UserId.Equals(userId));
}