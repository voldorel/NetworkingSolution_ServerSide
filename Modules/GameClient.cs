using System.Net.WebSockets;
using WebSocketsSample.Controllers;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
namespace GameServer.Modules
{
    public class GameClient : IDisposable
    {
        SafeHandle _handle = new SafeFileHandle(IntPtr.Zero, true);
        bool _disposed = false;
        private WebSocket _socket { get; set; } 
        private GameSession _currentSession { get; set; }
        private GameLobby _currentLobby { get; set; }
        private GameUser _currentUser { get; set; }
        public GameClient(WebSocket socket) => _socket = socket;


        public GameLobby GetCurrentLobby()
        {
            return _currentLobby;
        }


        public void AssignSocket(WebSocket webSocket)
        {
            try
            {
                if (webSocket != null)
                   _socket = webSocket;
            } catch
            {
                Console.WriteLine("assiging socket failed");
            }
        }

        public bool SetLobby(GameLobby gameLobby)
        {
            try
            {
                if (gameLobby != null)
                {
                    _currentLobby = gameLobby;
                    return true;
                }

            } catch
            {

            }
            return false;
        }

        public async Task SendLobbyJoinSuccess()
        {
            try
            {
                await WebSocketController.SendLobbySuccessfulJoin(_socket);
            }
            catch
            {

            }
        }


        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _handle.Dispose();
            }

            _disposed = true;
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

        public void ClearAssignedLobby()
        {
            _currentLobby = null;
        }

        public void CreateNewUser(string username)
        {
            Console.WriteLine("new username successfuly registered with this name: " +  username);
            _currentUser = new GameUser(username, "", "");
        }

        public WebSocket GetSocket()
        {
            return _socket;
        }

        public string GetUsername()
        {
            return _currentUser.GetUsername();
        }

        public void SetGameUser(GameUser gameUser)
        {
            _currentUser = gameUser;
        }

        public string GetUserId()
        {
            //needs to return actual user id
            return _currentUser.GetUsername();
        }

        public GameUser GetUser()
        {
            return _currentUser;
        }
    }
}

public struct GameUser
{
    private string _username;
    private string _password;
    private string _userId;

    public GameUser(string userName, string password, string userId)
    {
        _username = userName;
        _password = password;
        _userId = userId;
    }

    public string GetUsername()
    {
        return _username;
    }
}