using System.Security.Claims;
using GameServer.Models;
using GameServer.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using Server.Services;
using SharedLibrary;
using SharedLibrary.Requests;
using UAParser;

namespace Server.Controllers; 
[ApiController]
[Route("[controller]")]
public class TestController: ControllerBase
{
    private readonly MongoDbAccountContext _mongoDbAccountContext;
    private UserRepository _userRepository;
    public TestController(UserRepository userRepository)
    {
        _userRepository = userRepository;
    }
    [HttpPost("test")]
    public async Task<IActionResult> Test()
    {
        string id = HttpContext.Request.Headers["Authorization"].ToString();
        var parser = Parser.GetDefault();
        ClientInfo clientInfo=parser.Parse(HttpContext.Request.Headers["User-Agent"]);
        Console.WriteLine(clientInfo.Device.Family);
        Console.WriteLine(clientInfo.OS.Family);
        
        Console.WriteLine(clientInfo.ToString());
       //  User user= await _mongoDbAccountService.GetAsync(id);
         // var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        Console.WriteLine("user"+HttpContext.Connection.RemoteIpAddress);
        Console.WriteLine("User id : "+id);
        Console.WriteLine("test : "+HttpContext.User.Identity);
     //   Console.WriteLine("#############  test usr id"+user.Id);
        return Ok("OK");
    }
    [HttpPost("EarnResource")]
    public async Task<IActionResult> EarnResource(EarnResourceRequest earnResourceRequest)
    {
        Console.WriteLine("user score valueeeeeeeeeeeeeeeeee "+earnResourceRequest.Value);
        Console.WriteLine("user score Typeeeeeeeeeeeeeeeeeee "+earnResourceRequest.ResourceType);
        // await _userRepository.EarnResource(ObjectId.Parse(earnResourceRequest.UserId),earnResourceRequest.ResourceType,earnResourceRequest.Value,earnResourceRequest.Description);
        return Ok("Earn Success !!!");
    }
}