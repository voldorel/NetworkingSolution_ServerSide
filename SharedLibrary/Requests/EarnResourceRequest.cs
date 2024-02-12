using MongoDB.Bson;

namespace SharedLibrary.Requests;

public class EarnResourceRequest
{
    public string UserId;
    public string Type;
    public int Value;
    public string Description;
}