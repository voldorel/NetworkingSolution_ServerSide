using System.Diagnostics;
using System.Net;
using System.Security.Claims;
using System.Web.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Sane.Http.HttpResponseExceptions;
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
    public async Task<IActionResult> Register(RegisterRequest registerRequest)
    {
        try
        {
            Debug.Assert(HttpContext.Connection.RemoteIpAddress != null, nameof(HttpContext.Connection.RemoteIpAddress) + " != null");
            var (success, content) =await _authService.Register(registerRequest,HttpContext.Connection.RemoteIpAddress.ToString());
            if (!success) return BadRequest(content);
            //Console.WriteLine("Auth Controller After Register Enter Login");
            // return await Login(new LoginAuthenticationRequest(content.Value.ToString(),registerRequest.DeviceId,registerRequest.IpAddress));
            Debug.Assert(content != null, nameof(content) + " != null");
            // return await Login(new LoginRequestContent(content.Value,registerRequestContent.DeviceId,registerRequestContent.IpAddress));
            return Ok(new ResponseContent<RegisterResponse>(ResponseStatus.Success,new RegisterResponse(content.Value)));
        }
        catch (Exception e)
        {
            Console.WriteLine("USER REGISTER FAILED!");
            throw new HttpResponseException(HttpStatusCode.Unauthorized, "Register failed");
        }
    }
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequestContent requestContent)
    {
        // var (success, content,userData) = await _authService.Login(Guid.Parse(request.LoginToken),request.DeviceId,request.IpAddress);
        // var (success, content,userData) = await _authService.Login(requestContent.LoginToken,requestContent.DeviceId);
        try
        {
            var (success, content,userData) = await _authService.Login(requestContent);
            if (!success) return BadRequest(content);
            return Ok(new ResponseContent<LoginResponse>(ResponseStatus.Success,new LoginResponse()));

        }
        catch
        {
            Console.WriteLine("USER LOGIN FAILED!");
            throw;
        }
        
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