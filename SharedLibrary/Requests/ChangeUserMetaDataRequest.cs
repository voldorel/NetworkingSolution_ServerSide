using Newtonsoft.Json;

namespace SharedLibrary.Requests;

public class ChangeUserMetaDataRequest
{
    [JsonProperty("Address")]
    public List<string> Address;
    [JsonProperty("Value")]
    public string Value;
    [JsonProperty("DataType")]
    public string DataType;
}