using GameServer.Infrastructure;
using GameServer.Models;
using MongoDB.Driver;


namespace GameServer.Repositories;

public class UserRepository 
{
    private readonly IMongoCollection<User> _userCollection;
    public UserRepository(MongoDbContext dbContext) 
    {
        _userCollection = dbContext.Users;
        var tokenIndexKeysDefinition = Builders<User>.IndexKeys.Ascending(user => user.LoginToken);
        var tokenIndexModel = new CreateIndexModel<User>(tokenIndexKeysDefinition);
        _userCollection.Indexes.CreateOne(tokenIndexModel);
        var idIndexKeysDefinition = Builders<User>.IndexKeys.Ascending(user => user.Id);
        var idIndexModel = new CreateIndexModel<User>(idIndexKeysDefinition);
        _userCollection.Indexes.CreateOne(idIndexModel);
    }
    public async Task<User> GetAsyncById(string userId) => await _userCollection.Find(user=> user.Id.Equals(userId)).SingleOrDefaultAsync();
    public async Task<User> GetAsyncByLoginToken(Guid loginToken) => await _userCollection.Find(user=>user.LoginToken.Equals(loginToken)).SingleOrDefaultAsync();
    public async Task CreateAsync(User user) =>
        await _userCollection.InsertOneAsync(user);
    public async Task UpdateAsync(string userId,User user) =>
        await _userCollection.ReplaceOneAsync(user=>user.Id.Equals(userId),user);
    public async Task DeleteAsync(string userId) =>
        await _userCollection.DeleteOneAsync(user => user.Id.Equals(userId));
}