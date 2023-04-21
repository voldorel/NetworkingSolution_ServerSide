﻿using System.Net.WebSockets;
using WebSocketsSample.Controllers;
namespace GameServer.Modules
{
    public class GameClient
    {
        private WebSocket _socket { get; set; } 
        private GameSession _currentSession { get; set; }
        private GameLobby _currentLobby { get; set; }
        private GameUser _currentUser { get; set; }
        public GameClient(WebSocket socket) => _socket = socket;


        public GameLobby GetCurrentLobby()
        {
            return _currentLobby;
        }

        public async Task SetLobby(GameLobby gameLobby)
        {
            try
            {
                if (gameLobby != null)
                {
                    _currentLobby = gameLobby;
                    await WebSocketController.SendLobbySuccessfulJoin(_socket);
                }

            } catch
            {

            }
        }

        public GameSession GetCurrentGameSession()
        {
            return _currentSession;
        }

        public void SetGameSession(GameSession gameSession)
        {
            if (gameSession != null)
            {
                _currentSession = gameSession;
            }
        }

        public void SetClientUser(string username)
        {
            Console.WriteLine("new username successfuly registered with this name: " +  username);
            _currentUser = new GameUser(username);
        }

        public WebSocket GetSocket()
        {
            return _socket;
        }
    }
}

public class GameUser
{
    private string _username;
    private string _password;
    private string _userId;

    public GameUser(string userName)
    {
        _username = userName;
    }
}