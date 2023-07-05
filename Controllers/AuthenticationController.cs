using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Server.Services;
using SharedLibrary.Requests;
using SharedLibrary.Responses;

namespace Server.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthenticationController : ControllerBase
{
    private readonly IAuthenticationService _authService;

    public AuthenticationController(IAuthenticationService authService)
    {
        _authService = authService;
    }
    [HttpPost("register")]
    public async Task<IActionResult> Register(AuthenticationRequest request)
    {
        // var (success, content) = _authService.Register(request.Username, request.Password);
        var (success, content) =await _authService.Register(request.PhoneNumber);
        if (!success) return BadRequest(content);
        Console.WriteLine("Auth Controller After Register Enter Login");
        return await Login(request);
    }
    [HttpPost("login")]
    public async Task<IActionResult> Login(AuthenticationRequest request)
    {
        var (success, content) = await _authService.Login(request.PhoneNumber);
        if (!success) return BadRequest(content);
        return Ok(new AuthenticationResponse()
        {
            Token = content
        });
    }
    [Authorize]
    [HttpPost("test")]
    public async Task<IActionResult> Test()
    {
        var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        Console.WriteLine("User : "+userId);
        return Ok(userId);
    }
}