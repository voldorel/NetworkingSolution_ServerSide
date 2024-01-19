namespace SharedLibrary.Requests;

public class LoginRequestContent
{
   public string LoginToken { get; set; }
   public string IpAddress{ get; set; }

   // public LoginRequestContent(Guid loginToken, string deviceId, string ipAddress)
   // {
   //    LoginToken = loginToken;
   //    IpAddress = ipAddress;
   // }
}