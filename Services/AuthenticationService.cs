using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using GameServer.Models;
using GameServer.Repositories;
using Microsoft.AspNetCore.Authentication;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using Server.Models;
using SharedLibrary;
using SharedLibrary.Requests;
using SharedLibrary.Responses;

namespace Server.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly Settings _settings;
    private readonly UserRepository _userRepository;
    public AuthenticationService(Settings settings, UserRepository userRepository)
    {
        _settings = settings;
        _userRepository = userRepository;
    }
    public async Task<(bool success, Guid? content)> Register(RegisterRequest registerRequest,string ipAddress)
    {
        Console.WriteLine("IP Address :  "+ipAddress);
        var user = new User
        {
            DeviceId = registerRequest.DeviceId,
            OS = registerRequest.OperatingSystem
        };
        user.GenerateLoginToken();
        await _userRepository.CreateAsync(user);
        return (true,user.LoginToken);
    }
    private async Task<string> GetLocationByIP(string IP)
    {
        string url = "http://api.ipstack.com/" + IP + "?access_key=beb132be78a97cf886ddda5574bf66ce";
        var request = System.Net.WebRequest.Create(url);
        
        using (WebResponse wrs = request.GetResponse())
        {
            using (Stream stream = wrs.GetResponseStream())
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    string json = reader.ReadToEnd();
                    var obj = JObject.Parse(json);
                    string city = (string)obj["city"];
                    string country = (string)obj["region_name"];                    
                    string countryCode = (string)obj["country_code"];
        
                    Console.WriteLine("Location :   "+countryCode + " - " + country +"," + city);
                    return (countryCode + " - " + country +"," + city);
                }}}


        return "";

    }
    // public async Task<(bool success,string payLoad, User userData)> Login(Guid loginToken,string deviceId,string ipAddress)
    public async Task<(bool success,string payLoad, User userData)> Login(LoginRequestContent loginRequestContent)
    {
        // var currentUser = await _userRepository._userCollection.Find(user => user.LoginToken == loginToken).SingleOrDefaultAsync();
        // var currentUser = await _userRepository._userCollection.Find(user => user.LoginToken == loginRequestContent.LoginToken).SingleOrDefaultAsync();
        var currentUser = await _userRepository.GetAsyncByLoginToken(loginRequestContent.LoginToken);
        if (currentUser == null)
        {
            return (false, "This User Name Not Available!!! ",null)!;
        }
        return (true,"Login Successfully !!!",currentUser); // todo edit this 
    }

    #region JWTToken
    public ClaimsIdentity AssembleClaimsIdentity(User user)
    {
        var subject = new ClaimsIdentity(new[]
        {
            new Claim("id", user.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier,user.Id.ToString())
        });
        return subject;
    }
    public string GenerateJwtToken(ClaimsIdentity subject)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        Console.WriteLine("bearer key :  "+_settings.BearerKey);
        var key = Encoding.ASCII.GetBytes(_settings.BearerKey);
        var tokenDescriptor = new SecurityTokenDescriptor()
        {
            Subject = subject,
            Expires = DateTime.Now.AddYears(10),
            SigningCredentials =
                new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
    

    #endregion

    public Task<AuthenticateResult> AuthenticateAsync(HttpContext context, string? scheme)
    {
        throw new NotImplementedException();
    }

    public Task ChallengeAsync(HttpContext context, string? scheme, AuthenticationProperties? properties)
    {
        throw new NotImplementedException();
    }
    public Task ForbidAsync(HttpContext context, string? scheme, AuthenticationProperties? properties)
    {
        throw new NotImplementedException();
    }
    public Task SignInAsync(HttpContext context, string? scheme, ClaimsPrincipal principal, AuthenticationProperties? properties)
    {
        throw new NotImplementedException();
    }

    public Task SignOutAsync(HttpContext context, string? scheme, AuthenticationProperties? properties)
    {
        throw new NotImplementedException();
    }
}

public interface IAuthenticationService
{
    Task<(bool success, Guid? content)> Register(RegisterRequest registerRequest,string ipAddress);
    // Task< (bool success,string payLoad, User userData)> Login(Guid loginToken,string deviceId,string ipAddress);
    Task< (bool success,string payLoad, User userData)> Login(LoginRequestContent loginRequestContent);
}

public static class AuthenticationHelpers
{
    // public static void ProvideSaltAndHash(this User user)
    // {
    //     var salt = GenerateSalt();
    //     // user.Salt = Convert.ToBase64String(salt);
    //     // user.PassWordHash = ComputeToHash(user.PassWordHash, user.Salt);
    // }
    // public static byte[] GenerateSalt()
    // {
    //     var randomNumber = RandomNumberGenerator.Create();
    //     var salt = new byte[24];
    //     randomNumber.GetBytes(salt);
    //     return salt;
    // }
    // public static string ComputeToHash(string passWord, string saltString)
    // {
    //     var salt = Convert.FromBase64String(saltString);
    //     using var hashGenerator = new Rfc2898DeriveBytes(passWord, salt);
    //     hashGenerator.IterationCount = 10101;
    //     var bytes = hashGenerator.GetBytes(24);
    //     return Convert.ToBase64String(bytes);
    // }
    public static void GenerateLoginToken(this User user)
    {
        
        var tokenValue=Guid.NewGuid();
        Console.WriteLine("token : "+tokenValue);
        user.LoginToken = tokenValue;
    }
}