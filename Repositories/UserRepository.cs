using GameServer.Infrastructure;
using GameServer.Models;
using MongoDB.Bson;
using MongoDB.Driver;


namespace GameServer.Repositories;

public class UserRepository 
{
    private readonly IMongoCollection<User> _userCollection;
    private readonly ResourcesRepository _resourcesRepository;
    public UserRepository(MongoDbContext dbContext,ResourcesRepository resourcesRepository)
    {
        _resourcesRepository = resourcesRepository;
        _userCollection = dbContext.Users;
        var tokenIndexKeysDefinition = Builders<User>.IndexKeys.Ascending(user => user.LoginToken);
        var tokenIndexModel = new CreateIndexModel<User>(tokenIndexKeysDefinition);
        _userCollection.Indexes.CreateOne(tokenIndexModel);
        var idIndexKeysDefinition = Builders<User>.IndexKeys.Ascending(user => user.Id);
        var idIndexModel = new CreateIndexModel<User>(idIndexKeysDefinition);
        _userCollection.Indexes.CreateOne(idIndexModel);
    }
    public async Task<User> GetAsyncById(string userId) => await _userCollection.Find(user=> user.Id.Equals(userId)).SingleOrDefaultAsync();
    public async Task<User> GetAsyncById(ObjectId userId) => await _userCollection.Find(user=> user.Id.Equals(userId)).SingleOrDefaultAsync();
    public async Task<User> GetAsyncByLoginToken(Guid loginToken) => await _userCollection.Find(user=>user.LoginToken.Equals(loginToken)).SingleOrDefaultAsync();
    public async Task CreateAsync(User user) =>
        await _userCollection.InsertOneAsync(user);
    public async Task UpdateAsync(string userId,User user) =>
        await _userCollection.ReplaceOneAsync(user=>user.Id.Equals(userId),user);
    public async Task DeleteAsync(string userId) =>
        await _userCollection.DeleteOneAsync(user => user.Id.Equals(userId));
    private async Task UpdateResourcesAsync(ObjectId userId, BsonDocument updatedResources)
    {
        var filter = Builders<User>.Filter.Eq<ObjectId>(user => user.Id, userId);
        var update = Builders<User>.Update.Set(user => user.Resources, updatedResources);

        await _userCollection.UpdateOneAsync(filter, update);
    }
    public async Task EarnResource(ObjectId userId, string resourceType, long earnValue,string description="")
    {
        Console.WriteLine($"user id  : {userId} type : {resourceType}");
        var user = await GetAsyncById(userId);
        if (user == null)
        {
            throw new Exception($"User with ID {userId} not found.");
        }
        long lastValue = 0;
        if (user.Resources.Contains(resourceType))
        {
            lastValue = user.Resources[resourceType].AsInt64;
        }
        user.Resources[resourceType] = lastValue + earnValue;
        var resource = new Resource(userId, resourceType,earnValue, description);
        var resourceId= await _resourcesRepository.CreateAsync(resource);
        Console.Write("Added Resource Id : "+resourceId);
        await UpdateResourcesAsync(userId, user.Resources);
    }
}