using System.Text;
using GameServer.BackgroundServices;
using GameServer.Infrastructure;
using GameServer.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Server.Models;
using Server.Services;

var builder = WebApplication.CreateBuilder(args);
#region DataBase
var settings = new Settings();
builder.Configuration.Bind("Settings",settings);
builder.Services.AddSingleton(settings);
builder.Services.Configure<MongoDBSettings>(builder.Configuration.GetSection("MongoDB"));
builder.Services.AddSingleton<MongoDbContext>();
builder.Services.AddSingleton<UserRepository>();
builder.Services.AddSingleton<ResourcesRepository>();
builder.Services.AddControllers().AddNewtonsoftJson();
builder.Services.AddScoped<IPlayerService,PlayerService>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(option =>
{
    option.TokenValidationParameters = new TokenValidationParameters()
    {
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(settings.BearerKey)),
        ValidateIssuerSigningKey = true,
        ValidateAudience = false,
        ValidateIssuer = false
        
    };
});
#endregion


builder.Services.AddControllers();
//builder.Services.AddHostedService<MainGameService>();
builder.Services.AddSingleton<MainGameService>();//adds service as singleton but in the next line, it changes it to ihostedservice
// it helps when trying to resolve this service inside controllers
builder.Services.AddHostedService<MainGameService>(provider => provider.GetRequiredService<MainGameService>() as MainGameService);
builder.Services.AddSingleton<GameSessionHandlerService>();
builder.Services.AddHostedService<GameSessionHandlerService>(provider => provider.GetRequiredService<GameSessionHandlerService>() as GameSessionHandlerService);
builder.Services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
builder.Services.AddHostedService<QueuedHostedService>();
builder.Services.AddHttpContextAccessor();//added for httpcontext tools to read ip address. not necessary

builder.WebHost.UseUrls("http://localhost:5000;http://odin:5000");
var app = builder.Build();

// <snippet_UseWebSockets>
var webSocketOptions = new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromMinutes(1)
};

app.UseWebSockets(webSocketOptions);
// </snippet_UseWebSockets>

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();

app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

app.Run();