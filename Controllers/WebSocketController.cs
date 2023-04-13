﻿using System.Net.WebSockets;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using GameServer.Modules;
using GameServer.BackgroundServices;
namespace WebSocketsSample.Controllers;


public class WebSocketController : ControllerBase
{
    private MainGameService _mainGameService;
    private GameSessionHandlerService _gameSessionHandlerService;
    private ILogger<WebSocketController> _logger;
    public WebSocketController(ILogger<WebSocketController> logger, MainGameService hostedService, GameSessionHandlerService gameSessionHandlerService)
    {
        _mainGameService = hostedService;
        _logger = logger;
        _gameSessionHandlerService = gameSessionHandlerService;
    }



    [HttpGet("/ws")]
    public async Task Get()
    {
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            
            using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            

            var socketFinishedTcs = new TaskCompletionSource<GameClient>();
            _mainGameService.AddSocket(webSocket, socketFinishedTcs);
            GameClient gameClient =  await socketFinishedTcs.Task;
            //add to game session
            //start comunicating to session
            var EchoFinishedTcs = new TaskCompletionSource<object>();
            await Echo(gameClient, EchoFinishedTcs);


            await socketFinishedTcs.Task;
            Console.WriteLine("Socket Closed!");
            _mainGameService.DeleteSocket(gameClient);
        }
        else
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
    }
    

    private async Task Echo(GameClient gameClient, TaskCompletionSource<object> socketFinishedTcs)
    {
        WebSocket webSocket = gameClient.GetSocket();
        byte[] buffer = new byte[1024 * 4];
        var receiveResult = await webSocket.ReceiveAsync(
            new ArraySegment<byte>(buffer), CancellationToken.None);






        bool hasLobbyMessage = false;
        while (!receiveResult.CloseStatus.HasValue)
        {
            try
            {
                hasLobbyMessage = false;
                string request = Encoding.UTF8.GetString(new ArraySegment<byte>(buffer, 0, receiveResult.Count));
                JToken jToken = FromBson<JToken>(request);
                string requestType = (string)jToken["RequestType"];
                _logger.LogInformation(requestType);
                if (requestType != null)
                {
                    if (requestType.Equals("LobbyMessage"))
                    {
                        hasLobbyMessage = true;
                    }
                }
                if (gameClient.GetCurrentLobby() != null)
                {
                    var allClients = gameClient.GetCurrentLobby().Clients;
                    if (hasLobbyMessage)
                    {
                        ArraySegment<byte> bytes = new ArraySegment<byte>(buffer, 0, receiveResult.Count);
                        foreach (GameClient t in allClients)
                        {
                            await t.GetSocket().SendAsync(
                            bytes,
                            receiveResult.MessageType,
                            receiveResult.EndOfMessage,
                            CancellationToken.None);
                        }
                    }
                }
                if (gameClient.GetCurrentGameSession() != null)
                {
                    if (requestType.Equals("NetworkFunctionCall"))
                    {
                        await _gameSessionHandlerService.DoNetworkFunctionCall(gameClient.GetCurrentGameSession(), request, gameClient);
                    }
                }
            }
            catch
            {
                Console.WriteLine("Exception happened!");
            }
            
            /*await webSocket.SendAsync(
                new ArraySegment<byte>(buffer, 0, receiveResult.Count),
                receiveResult.MessageType,
                receiveResult.EndOfMessage,
                CancellationToken.None);*/

            receiveResult = await webSocket.ReceiveAsync(
                new ArraySegment<byte>(buffer), CancellationToken.None);

            

        }




        await webSocket.CloseAsync(
            receiveResult.CloseStatus.Value,
            receiveResult.CloseStatusDescription,
            CancellationToken.None);
        socketFinishedTcs.SetResult(socketFinishedTcs.Task);
    }

    
    public static async Task BroadCastLobbyMessage(GameLobby gameLobby, MessageType messageType, string message = "", int delay = 0)
    {
        if (delay != 0)
            await Task.Delay(delay);
        var allClients = gameLobby.Clients;
        if (allClients != null)
        {
            //byte[] buffer = new byte[1024 * 4];

            JObject jObject = new JObject();
            jObject.Add("Content", message);
            jObject.Add("RequestType", messageType.ToString());
            var bsonObject = ToBson(jObject);
            var encoded = Encoding.UTF8.GetBytes(bsonObject);
            foreach (GameClient t in allClients)
            {
                WebSocket websocket = t.GetSocket();
                if (websocket != null)
                {
                    if (websocket.State == WebSocketState.Open)
                    {
                        await websocket.SendAsync(encoded, WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                }
            }
        }
    }

    public static async Task SendSingleSessionMessage(GameSession gameSession, MessageType messageType, string message = "", GameClient targetClient = null)
    {
        JObject jObject = new JObject();
        jObject.Add("Content", message);
        jObject.Add("RequestType", messageType.ToString());
        var bsonObject = ToBson(jObject);
        var encoded = Encoding.UTF8.GetBytes(bsonObject);
        WebSocket websocket = targetClient.GetSocket();
        if (websocket != null)
        {
            if (websocket.State == WebSocketState.Open)
            {
                await websocket.SendAsync(encoded, WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }
    }

    public static async Task BroadCastSessionMessage(GameSession gameSession, MessageType messageType, string message = "", int timerCount = 0)
    {
        var allClients = gameSession.GetGameClients();
        if (allClients != null)
        {
            if (messageType == MessageType.SessionTimerUpdate)
            {
                foreach (GameClient t in allClients)
                {
                    byte[] bytes = BitConverter.GetBytes(timerCount);
                    if (BitConverter.IsLittleEndian)
                        Array.Reverse(bytes);
                    WebSocket websocket = t.GetSocket();
                    if (websocket != null)
                    {
                        if (websocket.State == WebSocketState.Open)
                        {
                            await websocket.SendAsync(bytes, WebSocketMessageType.Binary, true, CancellationToken.None);
                        }
                    }
                }
            }

            else
            {
                //byte[] buffer = new byte[1024 * 4];

                JObject jObject = new JObject();
                jObject.Add("Content", message);
                jObject.Add("RequestType", messageType.ToString());
                var bsonObject = ToBson(jObject);
                var encoded = Encoding.UTF8.GetBytes(bsonObject);
                foreach (GameClient t in allClients)
                {
                    WebSocket websocket = t.GetSocket();
                    if (websocket != null)
                    {
                        if (websocket.State == WebSocketState.Open)
                        {
                            await websocket.SendAsync(encoded, WebSocketMessageType.Text, true, CancellationToken.None);
                        }
                    }
                }
            }

        }
    }

    public static string ToBson<T>(T value)
    {
        using (MemoryStream ms = new MemoryStream())
        using (BsonDataWriter datawriter = new BsonDataWriter(ms))
        {
            JsonSerializer serializer = new JsonSerializer();
            serializer.Serialize(datawriter, value);
            return Convert.ToBase64String(ms.ToArray());
        }
    }

    public static T FromBson<T>(string base64data)
    {
        byte[] data = Convert.FromBase64String(base64data);

        using (MemoryStream ms = new MemoryStream(data))
        using (BsonDataReader reader = new BsonDataReader(ms))
        {
            JsonSerializer serializer = new JsonSerializer();
#pragma warning disable CS8603 // Possible null reference return.
            return serializer.Deserialize<T>(reader);
#pragma warning restore CS8603 // Possible null reference return.
        }
    }

    static object Deserialize(byte[] buffer)
    {
        using (StreamReader sr = new StreamReader(new MemoryStream(buffer)))
        {
            var result = JsonConvert.DeserializeObject<byte>(sr.ReadToEnd());
            return result;
        }
    }


}

public enum MessageType
{
    MatchMakingSuccess,
    GameFinished,
    SessionTimerUpdate,
    NetworkFunctionCall,
}