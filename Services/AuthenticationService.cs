using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using Server.Models;
using SharedLibrary;
namespace Server.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly Settings _settings;
    private readonly MongoDBAccountService _mongoDbAccountService;
    public AuthenticationService(Settings settings, MongoDBAccountService mongoDbAccountService)
    {
        _settings = settings;
        _mongoDbAccountService = mongoDbAccountService;
    }
    public async Task<(bool success, Guid? content)> Register(string os,string deviceId,string ipAddress)
    {
        // var currentUser = await _mongoDbAccountService._userCollection.Find(u => u.PhoneNumber == phoneNumber).SingleOrDefaultAsync();
        // if (currentUser != null)
        // {
        //     return (false, "This User Name Not Available!!! ");
        // }
        string location = await GetLocationByIP(ipAddress);
        Console.WriteLine("ip : "+ipAddress+ "   Location "+location);
        var user = new User
        {
            OS = os,
            DeviceId = deviceId,
            Location = location
        };
        user.GenerateLoginToken();
        _mongoDbAccountService.CreateAsync(user);
        Console.WriteLine("UID : "+user.Id);
       // user.ProvideSaltAndHash();
        // _context.Add(user);
        // _context.SaveChanges();
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
                    string City = (string)obj["city"];
                    string Country = (string)obj["region_name"];                    
                    string CountryCode = (string)obj["country_code"];
        
                    Console.WriteLine("Location :   "+CountryCode + " - " + Country +"," + City);
                    return (CountryCode + " - " + Country +"," + City);
                }}}


        return "";

    }
    public async Task<(bool success,string payLoad, User userData)> Login(Guid loginToken,string deviceId,string ipAddress)
    {
        var currentUser = await _mongoDbAccountService._userCollection.Find(user => user.LoginToken == loginToken).SingleOrDefaultAsync();
        if (currentUser == null)
        {
            return (false, "This User Name Not Available!!! ",null);
        }
        // if (user.PassWordHash != AuthenticationHelpers.ComputeToHash(passWord, user.Salt))
        //     return (false, "Invalid Password ");
        Console.WriteLine("Login In Auth Service Generate JWT Token");
         return (true,"Login Successfully !!!",currentUser); // todo edit this 
    }
    public ClaimsIdentity AssembleClaimsIdentity(User user)
    {
        var subject = new ClaimsIdentity(new[]
        {
            new Claim("id", user.Id),
            new Claim(ClaimTypes.NameIdentifier,user.Id)
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
    Task<(bool success, Guid? content)> Register(string os,string deviceId,string ipAddress);
    Task< (bool success,string payLoad, User userData)> Login(Guid loginToken,string deviceId,string ipAddress);
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