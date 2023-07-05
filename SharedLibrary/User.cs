using System.ComponentModel.DataAnnotations;
using System.Data;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SharedLibrary;
public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonRequired]
    public string PhoneNumber;
    public string NickName { get; set; }
    public BsonDocument Resources;
}