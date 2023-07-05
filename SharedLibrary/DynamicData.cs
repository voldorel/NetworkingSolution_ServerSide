using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SharedLibrary;

public class DynamicData
{
    public BsonDocument Data { get; set; }
}