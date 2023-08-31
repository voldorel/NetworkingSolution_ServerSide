namespace SharedLibrary.Requests;

public class LoginAuthenticationRequest
{
   public string LoginToken { get; set; }
   public string DeviceId{ get; set; }
   public string IpAddress{ get; set; }

   public LoginAuthenticationRequest(string loginToken, string deviceId, string ipAddress)
   {
      LoginToken = loginToken;
      DeviceId = deviceId;
      IpAddress = ipAddress;
   }
}