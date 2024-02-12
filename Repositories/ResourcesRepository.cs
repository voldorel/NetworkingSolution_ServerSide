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
        var indexKeysDefinition = Builders<Resource>.IndexKeys.Ascending(resource => resource.Id);
        var tokenIndexModel = new CreateIndexModel<Resource>(indexKeysDefinition);
        _resourcesCollection.Indexes.CreateOne(tokenIndexModel);
    }
    public async Task<List<Resource>> GetAsync() =>
        await _resourcesCollection.Find(_ => true).ToListAsync();
    public async Task<Resource> GetAsync(ObjectId resourceId) =>
        await _resourcesCollection.Find(resource=>resource.Id.Equals(resourceId)).FirstOrDefaultAsync();

    public async Task<ObjectId> CreateAsync(Resource resource)
    {
        await _resourcesCollection.InsertOneAsync(resource);
        return resource.Id;
    }

    public async Task DeleteAsync(string resourceId) =>
        await _resourcesCollection.DeleteOneAsync(resource => resource.Id.Equals(resourceId));
    public async Task<int> UpdateAsync(ObjectId id, int newValue)
    {
        var filter = Builders<Resource>.Filter.Eq(resource => resource.Id, id);
        var update = Builders<Resource>.Update
            .Set(resource => resource.Value, newValue);
        var result = await _resourcesCollection.UpdateOneAsync(filter, update);
        if (result.IsAcknowledged && result.ModifiedCount > 0)
        {
            return newValue; // Return the new value if the update was successful
        }
        throw new Exception($"Failed to update resource with ID {id}");
    }
}