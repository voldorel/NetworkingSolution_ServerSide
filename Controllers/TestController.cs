﻿using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using Server.Services;
using SharedLibrary;
using SharedLibrary.Requests;

namespace Server.Controllers;
[Authorize]
[ApiController]
[Route("[controller]")]
public class TestController: ControllerBase
{
    private readonly MongoDBAccountService _mongoDbAccountService;
    private readonly MongoDBResourecesService _mongoDbResourecesService;

    public TestController(MongoDBAccountService mongoDbAccountService,MongoDBResourecesService mongoDbResourecesService)
    {
        _mongoDbResourecesService = mongoDbResourecesService;
        _mongoDbAccountService = mongoDbAccountService;
        Console.WriteLine("Player Controller Init ");
    }
    [HttpPost("test")]
    public async Task<IActionResult> Test()
    {
        string id = HttpContext.User.Claims.First(c => c.Type == "id").Value;
         User user= await _mongoDbAccountService.GetAsync(id);
         var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        Console.WriteLine("user"+HttpContext.Connection.RemoteIpAddress);
        Console.WriteLine("User : "+user.PhoneNumber);
        Console.WriteLine("User id : "+id);
        Console.WriteLine("test : "+HttpContext.User.Identity);
        return Ok(userId);
    }
    [HttpPost("UpdateScore")]
    public async Task<IActionResult> AddScoreToPlayer([FromBody] UpdateResourceRequest updateResourceRequest)
    {
        Console.WriteLine("tst "+HttpContext.Request.Headers["User-Agent"].ToString());
        Console.WriteLine("UpdateScore : "+updateResourceRequest.ResourceName);
        string userId=HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        Console.WriteLine("User Id : "+userId);
        User user = await _mongoDbAccountService.GetAsync(userId);
        if (user != null)
        {
            Console.WriteLine("User Id : " + user.Id + "  Resource : ");
            if (user.Resources.IsNullOrEmpty())
            {
                Console.WriteLine("User Resources Is Null Or Empty ");
                Resources currentResources = new Resources()
                {
                    UserId = userId,
                    Data = new BsonDocument(),
                };
                currentResources.Data[updateResourceRequest.ResourceName] = updateResourceRequest.Value;
                await _mongoDbResourecesService.CreateAsync(currentResources);
                user.Resources = currentResources.Data;
                _mongoDbAccountService.UpdateAsync(userId, user);
            }
            else
            {
                Console.WriteLine("User Have Resourcs In Past ");
                user.Resources[updateResourceRequest.ResourceName] = updateResourceRequest.Value;
                await _mongoDbResourecesService.UpdateAsync(user.Id,
                    new Resources() { UserId = user.Id, Data = user.Resources });
                await _mongoDbAccountService.UpdateAsync(userId, user);
            }
        }

        return Ok("Update Resources Successfully : " + updateResourceRequest.Value);
    }
}