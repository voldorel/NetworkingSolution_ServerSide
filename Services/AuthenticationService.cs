using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using MongoDB.Driver;
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
    public async Task<(bool success, string content)> Register(string phoneNumber)
    {
        var currentUser = await _mongoDbAccountService._userCollection.Find(u => u.PhoneNumber == phoneNumber).SingleOrDefaultAsync();
        if (currentUser != null)
        {
            return (false, "This User Name Not Available!!! ");
        }
        var user = new User()
        {
            PhoneNumber = phoneNumber,
            //UserName = userName,
          // PassWordHash = "Taha7928"
        };
        _mongoDbAccountService.CreateAsync(user);
        Console.WriteLine("UID : "+user.Id);
       // user.ProvideSaltAndHash();
        // _context.Add(user);
        // _context.SaveChanges();
        return (true, "");
    }
    public async Task<(bool success, string token)> Login(string phoneNumber)
    {
        var currentUser = await _mongoDbAccountService._userCollection.Find(u => u.PhoneNumber == phoneNumber).SingleOrDefaultAsync();
        if (currentUser == null)
        {
            return (false, "This User Name Not Available!!! ");
        }
        // if (user.PassWordHash != AuthenticationHelpers.ComputeToHash(passWord, user.Salt))
        //     return (false, "Invalid Password ");
        Console.WriteLine("Login In Auth Service Generate JWT Token");
         return (true, GenerateJwtToken(AssembleClaimsIdentity(currentUser))); // todo edit this 
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
    Task<(bool success, string content)> Register(string phoneNumber);
    Task< (bool success, string token)> Login(string phoneNumber);
}

public static class AuthenticationHelpers
{
    public static void ProvideSaltAndHash(this User user)
    {
        var salt = GenerateSalt();
        // user.Salt = Convert.ToBase64String(salt);
        // user.PassWordHash = ComputeToHash(user.PassWordHash, user.Salt);
    }
    public static byte[] GenerateSalt()
    {
        var randomNumber = RandomNumberGenerator.Create();
        var salt = new byte[24];
        randomNumber.GetBytes(salt);
        return salt;
    }
    public static string ComputeToHash(string passWord, string saltString)
    {
        var salt = Convert.FromBase64String(saltString);
        using var hashGenerator = new Rfc2898DeriveBytes(passWord, salt);
        hashGenerator.IterationCount = 10101;
        var bytes = hashGenerator.GetBytes(24);
        return Convert.ToBase64String(bytes);
    }
}