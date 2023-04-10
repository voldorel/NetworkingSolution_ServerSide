using System.Net.WebSockets;
namespace GameServer.Modules
{
    public class GameClient
    {
        private WebSocket _socket { get; set; } 
        private GameSession _currentSession { get; set; }
        private GameLobby _currentLobby { get; set; }
        public GameClient(WebSocket socket) => _socket = socket;



        public GameLobby GetCurrentLobby()
        {
            return _currentLobby;
        }

        public void SetLobby(GameLobby gameLoby)
        {
            if (gameLoby != null)
            {
                _currentLobby = gameLoby;
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