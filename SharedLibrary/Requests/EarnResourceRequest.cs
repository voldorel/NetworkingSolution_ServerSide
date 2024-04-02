using MongoDB.Bson;
using Newtonsoft.Json;

namespace SharedLibrary.Requests;

public class EarnResourceRequest
{
    [JsonProperty("ResourceType")]
    public string ResourceType;
    [JsonProperty("Value")]
    public int Value;
    [JsonProperty("Description")]
    public string Description;
}