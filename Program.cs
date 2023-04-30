using GameServer.BackgroundServices;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
//builder.Services.AddHostedService<MainGameService>();
builder.Services.AddSingleton<MainGameService>();//adds service as singleton but in the next line, it changes it to ihostedservice
// it helps when trying to resolve this service inside controllers
builder.Services.AddHostedService<MainGameService>(provider => provider.GetRequiredService<MainGameService>() as MainGameService);


builder.Services.AddSingleton<GameSessionHandlerService>();
builder.Services.AddHostedService<GameSessionHandlerService>(provider => provider.GetRequiredService<GameSessionHandlerService>() as GameSessionHandlerService);




builder.Services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
builder.Services.AddHostedService<QueuedHostedService>();

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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}