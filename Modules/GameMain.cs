using System.Net.WebSockets;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using WebSocketsSample.Controllers;
namespace GameServer.Modules
{
    
    public class GameMain 
    {
        /*public static GameMain Instance = new GameMain();
        //private List<WebSocket> _sockets = new List<WebSocket>();
        private static int usingResource = 0;
        //private ConcurrentQueue<GameLobby> _lobbies = new ConcurrentQueue<GameLobby>();
        //private ConcurrentQueue<GameSession> _sessions = new ConcurrentQueue<GameSession>();
        private List<GameLobby> _lobbies = new List<GameLobby>();
        private List<GameSession> _sessions = new List<GameSession>();

       

        internal async void AddSocket(WebSocket webSocket, TaskCompletionSource<GameClient> socketFinishedTcs)
        {
            //should change this
            //for now there's just gonna be a single lobby 
            GameClient gameClient = new GameClient(webSocket);
            if (Interlocked.Exchange(ref usingResource, 1) == 0)
            {
                if (_lobbies.Count == 0)
                {
                    GameLobby gameLobby = new GameLobby(gameClient);
                    gameClient.SetLobby(gameLobby);
                    _lobbies.Add(gameLobby);
                     
                    //debug section here for starting session with only one player
                } else
                {
                    var gameLobby = _lobbies.FirstOrDefault();
                    if (gameLobby != null)
                    {
                        gameClient.SetLobby(gameLobby);
                        gameLobby.AddClient(gameClient);
                        if (gameLobby.GetClientCount() >= 10) //hard coded user count must be changed into a variable type
                        {
                            // create game session now and remove the lobby. MUST do this asap. getting to start the game session logic
                            GameSession session = new GameSession(gameLobby.Clients);
                            await WebSocketController.BroadCastMessage(gameLobby, MessageType.MatchMakingSuccess, "", 3000);
                            CloseLobby(gameLobby);
                        }
                    }
                }
                Console.WriteLine("New User Just Logged in!");
                Interlocked.Exchange(ref usingResource, 0);
            }
            socketFinishedTcs.SetResult(gameClient);
        }
        internal void DeleteSocket(GameClient gameClient)
        {
            //update this later to support game session
            try
            {
                if (Interlocked.Exchange(ref usingResource, 1) == 0)
                {
                    var gameLobby = gameClient.GetCurrentLobby();
                    gameLobby.RemoveClient(gameClient);
                    CloseLobby(gameLobby);
                    Interlocked.Exchange(ref usingResource, 0);
                }
            } catch { }
        }

        internal void CloseLobby(GameLobby gameLobby)
        {
            if (gameLobby != null)
            {
                if (gameLobby.GetClientCount() == 0)
                {
                    _lobbies.Remove(gameLobby);
                    gameLobby = null;
                }
            }
        }*/
    }
}
