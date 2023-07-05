using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Server.Models;
using SharedLibrary;

namespace Server.Services;

public class MongoDBAccountService : MongoDBService
{
    public readonly IMongoCollection<User> _userCollection;
    public MongoDBAccountService(IOptions<MongoDBSettings> mongoDBSettings) : base(mongoDBSettings)
    {
        CollectionName = "Accounts";
        _userCollection = Database.GetCollection<User>(CollectionName);
    }
    public async Task<List<User>> GetAsync() =>
        await _userCollection.Find(_ => true).ToListAsync();
    public async Task<User> GetAsync(string userId) =>
        await _userCollection.Find(user=>user.Id.Equals(userId)).FirstOrDefaultAsync();
        public async Task CreateAsync(User user) =>
        await _userCollection.InsertOneAsync(user);
    public async Task UpdateAsync(string userId,User user) =>
        await _userCollection.ReplaceOneAsync(user=>user.Id.Equals(userId),user);
    public async Task DeleteAsync(string userId) =>
        await _userCollection.DeleteOneAsync(user => user.Id.Equals(userId));
}