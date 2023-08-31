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
    [BsonRepresentation(BsonType.String)]
    public Guid? LoginToken { get; set; }
    public string NickName { get; set; }
    public string DeviceId;
    public string OS;
    public string Location;
    public BsonDocument Resources;
}