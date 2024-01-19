using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GameServer.Models;
public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public ObjectId Id { get; set; }
    [BsonRepresentation(BsonType.String)]
    public Guid LoginToken { get; set; }
    public string NickName { get; set; }
    public string? DeviceId;
    public string? Location;
    public string? OS;
    public BsonDocument Resources;
}