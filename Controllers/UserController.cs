using Microsoft.AspNetCore.Mvc;
using Server.Services;
using SharedLibrary;

namespace Server.Controllers;
[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    private readonly MongoDBAccountService _mongoDbAccountService;
    public UserController(MongoDBAccountService mongoDbAccountService)
    {
        _mongoDbAccountService = mongoDbAccountService;
        Console.WriteLine("Player Controller Init ");
        User user = new User()
        {
            NickName = "taha",
        };
        // Console.WriteLine("User Id : "+user.UId);
        // _context.Add(user);
        // _context.SaveChanges();
    }
    // [HttpGet("{id}")]
    // public Player Get([FromRoute] int id)
    // {
    //     _playerService.DoSomething();
    //     return new Player(){Id = id};
    // }
    //
    // [HttpPost]
    // public Player Post(Player player)
    // {
    //     Console.WriteLine("Player Has Been Added \n  Level :  "+player.Level);
    //     return player;
    // }
    //
    
    [HttpGet]
    public async Task<List<User>> Get()
    {
        Console.WriteLine("Get In User Controller ");
        return null;
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] User user)
    {
        return null;
    }

    // [HttpPut("{id}")]
    // public async Task<IActionResult> AddToPlaylist(string id, [FromBody] string movieId) {}

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        return null;
    }
}