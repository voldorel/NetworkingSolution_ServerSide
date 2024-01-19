namespace SharedLibrary.Requests;

public class RegisterRequest
{
    public string OperatingSystem{ get; set; }
    public string DeviceId{ get; set; }

    public RegisterRequest(string operatingSystem, string deviceId)
    {
        OperatingSystem = operatingSystem;
        DeviceId = deviceId;
    }
}