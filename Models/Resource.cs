using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GameServer.Models;

public class Resource
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public ObjectId UserId { get; set; }
    public BsonDocument Data { get; set; }
}