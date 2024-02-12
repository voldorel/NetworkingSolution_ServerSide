using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GameServer.Models;

public class Resource
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public ObjectId Id { get; set; }
    public ObjectId UserId;
    public string Type { get; set; }
    public long Value { get; set; }
    public DateTime EarnDateTime { get; set; }
    public string Description;

    public Resource(ObjectId userId, string type, long value, string description)
    {
        this.UserId = userId;
        this.Type = type;
        this.Value = value;
        this.Description = description;
        EarnDateTime=DateTime.UtcNow;
    }
}