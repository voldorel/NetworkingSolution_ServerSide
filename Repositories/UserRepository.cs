using GameServer.Infrastructure;
using GameServer.Models;
using Microsoft.OpenApi.Any;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using SharedLibrary.Requests;


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
    public async Task EarnResource(ObjectId userId, string resourceType, long earnValue,string description,Action<string> OnComplete)
    {
        Console.WriteLine($"user id earn resource : {userId} type : {resourceType}");
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

        JObject modifyResource = new JObject();
        modifyResource.Add(resourceType,user.Resources[resourceType].AsInt64);
        OnComplete?.Invoke(modifyResource.ToString());
    }

    public async Task ChangeUserMetaData(ObjectId userId,ChangeUserMetaDataRequest userMetaDataRequest)
    {
        Console.WriteLine($"user id change user meta data : {userId}");
        var user = await GetAsyncById(userId);
        if (user == null)
        {
            throw new Exception($"User with ID {userId} not found.");
        }

        // BsonDocument userMetaData = user.UserMetaData;
        int index = 0;
        int addressCount = userMetaDataRequest.Address.Count;
        BsonDocument current = user.UserMetaData;
        for (int i = 0; i < addressCount - 1; i++)
        {
            string key = userMetaDataRequest.Address[i];
            if (!current.Contains(key) )
            {
                // current[key] = new BsonDocument();
                current.Add(key, new BsonDocument());
            }

            if (!current[key].IsBsonDocument)
            {
                current[key] = new BsonDocument();
            }
            current = current[key].AsBsonDocument;
        }
        string lastKey = userMetaDataRequest.Address[addressCount-1];
        object value = userMetaDataRequest.Value;
        if (current.Contains(lastKey))
        {
            current[lastKey] = userMetaDataRequest.Value;
        }
        else
        {
            current.Add(lastKey, userMetaDataRequest.Value);
        }
        var filter = Builders<User>.Filter.Eq<ObjectId>(filterUser => filterUser.Id, userId);
        var update = Builders<User>.Update.Set(updateUser => updateUser.UserMetaData,user.UserMetaData);
        await _userCollection.UpdateOneAsync(filter, update);
    }

    public async Task ChangeUserNickName(ObjectId userId,string nickname)
    {
        var filter = Builders<User>.Filter.Eq<ObjectId>(filterUser => filterUser.Id, userId);
        var update = Builders<User>.Update.Set(updateUser => updateUser.NickName,nickname);
        await _userCollection.UpdateOneAsync(filter, update);
    }
    
    public async Task ChangeAvatar(ObjectId userId,int avatarId,Action<string> OnComplete=null)
    {
        var user = await GetAsyncById(userId);
        if (user == null)
        {
            throw new Exception($"User with ID {userId} not found.");
        }

        if (user.ProfileMetaData.Contains("AvatarId"))
        {
            user.ProfileMetaData["AvatarId"] = avatarId;
        }
        else
        {
            user.ProfileMetaData.Add("AvatarId", avatarId);
        }
        var filter = Builders<User>.Filter.Eq<ObjectId>(filterUser => filterUser.Id, user.Id);
        var update = Builders<User>.Update.Set(updateUser => updateUser.ProfileMetaData,user.ProfileMetaData);
        await _userCollection.UpdateOneAsync(filter, update);
        OnComplete?.Invoke(user.ProfileMetaData.ToString());
    }
}