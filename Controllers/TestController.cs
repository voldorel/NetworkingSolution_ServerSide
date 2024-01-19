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
    private readonly ResourcesRepository _resourcesRepository;

    public TestController(MongoDbAccountContext mongoDbAccountContext,ResourcesRepository resourcesRepository)
    {
        _resourcesRepository = resourcesRepository;
        _mongoDbAccountContext = mongoDbAccountContext;
        Console.WriteLine("Player Controller Init ");
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
    [HttpPost("UpdateScore")]
    public async Task<IActionResult> AddScoreToPlayer([FromBody] UpdateResourceRequest updateResourceRequest)
    {
        Console.WriteLine("tst "+HttpContext.Request.Headers["User-Agent"].ToString());
        Console.WriteLine("UpdateScore : "+updateResourceRequest.ResourceName);
        string userId=HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        Console.WriteLine("User Id : "+userId); 
        User user = await _mongoDbAccountContext.GetAsync(userId);
        if (user != null)
        {
            Console.WriteLine("User Id : " + user.Id + "  Resource : ");
            if (user.Resources.IsNullOrEmpty())
            {
                Console.WriteLine("User Resources Is Null Or Empty ");
                Resource currentResource = new Resource()
                {
                    UserId = ObjectId.Parse(userId),
                    Data = new BsonDocument(),
                };
                currentResource.Data[updateResourceRequest.ResourceName] = updateResourceRequest.Value;
                await _resourcesRepository.CreateAsync(currentResource);
                user.Resources = currentResource.Data;
                _mongoDbAccountContext.UpdateAsync(userId, user);
            }
            else
            {
                Console.WriteLine("User Have Resourcs In Past ");
                user.Resources[updateResourceRequest.ResourceName] = updateResourceRequest.Value;
                await _resourcesRepository.UpdateAsync(user.Id,
                    new Resource() { UserId = user.Id, Data = user.Resources });
                await _mongoDbAccountContext.UpdateAsync(userId, user);
            }
        }

        return Ok("Update Resources Successfully : " + updateResourceRequest.Value);
    }
}