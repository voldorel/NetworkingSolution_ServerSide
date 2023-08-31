namespace SharedLibrary.Requests;

public class RegisterAuthenticationRequest
{
    public string OperatingSystem{ get; set; }
    public string DeviceId{ get; set; }
    public string IpAddress{ get; set; }

    public RegisterAuthenticationRequest(string operatingSystem, string deviceId, string ipAddress)
    {
        OperatingSystem = operatingSystem;
        DeviceId = deviceId;
        IpAddress = ipAddress;
    }
}