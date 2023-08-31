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
    public async Task<IActionResult> Register(RegisterAuthenticationRequest registerRequest)
    {
        // var (success, content) = _authService.Register(request.Username, request.Password);
        var (success, content) =await _authService.Register(registerRequest.OperatingSystem,registerRequest.DeviceId,registerRequest.IpAddress);
        if (!success) return BadRequest(content);
        Console.WriteLine("Auth Controller After Register Enter Login");
        return await Login(new LoginAuthenticationRequest(content.Value.ToString(),registerRequest.DeviceId,registerRequest.IpAddress));
    }
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginAuthenticationRequest request)
    {
        var (success, content,userData) = await _authService.Login(Guid.Parse(request.LoginToken),request.DeviceId,request.IpAddress);
        if (!success) return BadRequest(content);
        return Ok(new AuthenticationLoginResponse()
        {
            LoginStatus = true,
            UserData=userData
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