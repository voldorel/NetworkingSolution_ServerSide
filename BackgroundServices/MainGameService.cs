using System.Net.WebSockets;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using WebSocketsSample.Controllers;
using GameServer.Modules;

namespace GameServer.BackgroundServices
{
    public class MainGameService : IHostedService, IDisposable
    {
        //public static MainGameService Instance = new MainGameService();
        //private List<WebSocket> _sockets = new List<WebSocket>();
        private static int usingResource = 0;
        private static int usingSessionResource = 0;
        //private ConcurrentQueue<GameLobby> _lobbies = new ConcurrentQueue<GameLobby>();
        //private ConcurrentQueue<GameSession> _sessions = new ConcurrentQueue<GameSession>();
        private List<GameLobby> _lobbies = new List<GameLobby>();
        private List<GameSession> _sessions = new List<GameSession>();
        private readonly ILogger<MainGameService> _logger;
        private int _executionCount = 0;
        private Timer? _serverUpTimer = null;
        private GameSessionHandlerService _sessionHandlerService;

        public MainGameService(ILogger<MainGameService> logger, GameSessionHandlerService gameSessionHandlerService)
        {
            _logger = logger;
            _sessionHandlerService = gameSessionHandlerService;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Timed Hosted Service running.");

            _serverUpTimer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));

            return Task.CompletedTask;
        }

        private void DoWork(object? state)
        {
            var count = Interlocked.Increment(ref _executionCount);

            //_logger.LogInformation("Timed Hosted Service is working. Count: {Count}", count);
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Timed Hosted Service is stopping.");

            _serverUpTimer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }



        internal void AddSocket(WebSocket webSocket, TaskCompletionSource<GameClient> socketFinishedTcs)
        {
            GameClient gameClient = new GameClient(webSocket);
            _logger.LogInformation("New User Just Logged in!");
            socketFinishedTcs.SetResult(gameClient);//was different...needs to be removed bc no need anymore for blocking call
        }
        internal bool LoginUser(string username, ref GameClient gameClient) ////////////this function needs to be rewritten based on db context
        {
            bool result = false;
            if (Interlocked.Exchange(ref usingSessionResource, 1) == 0)
            {
                try
                {

                    if (_sessions.Count == 0)
                    {
                        gameClient.CreateNewUser(username);

                    }
                    else
                    {
                        foreach (GameSession gameSession in _sessions)
                        {
                            GameClient targetClient = gameSession.GetGameClients().Find(i => i.GetUsername().Equals(username));
                            bool ClientExists = targetClient != null;
                            if (ClientExists)
                            {
                                //Console.WriteLine(targetClient.GetUsername());
                                targetClient.AssignSocket(gameClient.GetSocket());//check if initially created game client and socket get deleted or not

                                /*if (gameClient.GetCurrentGameSession() != null)
                                {
                                    gameClient.SetGameSession(gameClient.GetCurrentGameSession());
                                    isInGameSession = true;
                                }*/
                                gameClient = targetClient;

                            }
                            else
                            {
                                gameClient.CreateNewUser(username);
                            }
                        }
                        /*JObject keyValuePairs = new JObject();
                        keyValuePairs.Add("IsInGameSession", isInGameSession);
                        keyValuePairs.Add("username", username);*/
                    }
                    result = true;
                }
                catch
                {
                    _logger.LogError("User Login Failed");
                }
                Interlocked.Exchange(ref usingSessionResource, 0);
            }
            return result;
        } 


        public async void JoinLobby(GameClient gameClient)
        {
            if (Interlocked.Exchange(ref usingResource, 1) == 0)
            {
                try
                {
                    bool joinSuccessful = false;
                    if (_lobbies.Count == 0)
                    {
                        GameLobby gameLobby = new GameLobby(gameClient);
                        joinSuccessful = gameClient.SetLobby(gameLobby);
                        _lobbies.Add(gameLobby);

                        //debug section here for starting session with only one player
                        if (false) //delete this after testing is finished. it causes problems if left undeleted
                        {
                            // create game session now and remove the lobby. MUST do this asap. getting to start the game session logic
                            StartGameSession(gameClient);

                            //we need to send all user data to all users who are present in the session
                        }
                    }
                    else
                    {
                        GameLobby gameLobby = _lobbies.FirstOrDefault();
                        if (gameLobby != null)
                        {
                            joinSuccessful = gameClient.SetLobby(gameLobby);
                            gameLobby.AddClient(gameClient);
                            if (gameLobby.GetClientCount() >= 10) //hard coded user count must be changed into a variable type
                            {
                                StartGameSession(gameClient);
                            }
                        }
                    }
                    if (joinSuccessful)
                    {
                        await gameClient.SendLobbyJoinSuccess();
                    }
                }
                catch
                {

                }
                Interlocked.Exchange(ref usingResource, 0);
            }
        }

        public async void StartGameSession(GameClient gameClient)
        {
            if (Interlocked.Exchange(ref usingSessionResource, 1) == 0)
            {
                try
                {
                    GameLobby gameLobby = gameClient.GetCurrentLobby();
                    GameSession gameSession = new GameSession(gameLobby.Clients);
                    gameSession.InitializeClients(gameSession);
                    _sessions.Add(gameSession);
                    //await WebSocketController.BroadCastLobbyMessage(gameLobby, MessageType.MatchMakingSuccess, "", 1000);
                    await WebSocketController.BroadCastLobbyMessage(gameLobby, MessageType.MatchMakingSuccess);
                    CloseLobby(gameLobby);
                    _sessionHandlerService.AddGameSession(gameSession);
                } catch
                {
                    _logger.LogInformation("start session failed");
                }
                Interlocked.Exchange(ref usingSessionResource, 0);
            }
            
        }


        public async void SendGameData(GameClient gameClient)
        {
            try
            {
                ///////////////////////////////this//part//is//wrong///////////////////////////////////////
                ///                      needs to be deleted asap
                ///                      here's what's happening:
                ///                      we're supposed to find user based on it's user_id in our database
                ///                      since we still haven't got any database as of yet
                ///                      we temporarily take a very wrong but functional approach
                ///                      we read through our on-going session's data
                ///                      we look at users and check their username and see if it matches the we got
                ///                      if so then this user can join the on-going match
                ///                      this is just for test purposes until proper database checks are added
                ///                      
                /// 
                ///                      also keep in mind...we need to keep track of a clients game session
                ///                      inside the database. meaening that we shouldn't itertate and check through
                ///                      all online clients just to find the user that we're looking for
                ///                      we should just check inside the database and know if a user is in a 
                ///                      game session which is open and on going
                ///////////////////////////////////////////////////////////////////////////////////////////
                foreach (GameSession gameSession in _sessions)
                {
                    foreach (GameClient sessionClient in gameSession.GetGameClients())
                    {
                        if (sessionClient.GetUsername().Equals(gameClient.GetUsername()))
                        {
                            sessionClient.AssignSocket(gameClient.GetSocket());
                            gameClient.SetGameSession(gameSession);
                            //should assign user as well but will be added after database completion
                        }
                    }

                }
                await WebSocketController.SendGameData(gameClient);
            }
            catch
            {
                _logger.LogInformation("user send data failed");
            }

        }


        internal void DeleteSocket(GameClient gameClient)
        {
            //update this later to support game session
            try
            {
                if (Interlocked.Exchange(ref usingResource, 1) == 0)
                {
                    GameLobby gameLobby = gameClient.GetCurrentLobby();
                    if (gameLobby != null)
                    {
                        gameLobby.RemoveClient(gameClient);
                        TryCloseLobby(gameLobby);
                    }
                    Interlocked.Exchange(ref usingResource, 0);
                }
                if (Interlocked.Exchange(ref usingSessionResource, 1) == 0)
                {
                    GameSession gameSession = gameClient.GetCurrentGameSession();
                    if (gameSession != null)
                    {
                        //broadcast member left event
                    }
                    Interlocked.Exchange(ref usingSessionResource, 0);
                }
            }
            catch { }
        }

        internal void TryCloseLobby(GameLobby gameLobby)
        {
            if (gameLobby != null)
            {
                if (gameLobby.GetClientCount() == 0)
                {
                    CloseLobby(gameLobby);
                }
            }
        }

        internal void CloseLobby(GameLobby gameLobby)
        {
            _lobbies.Remove(gameLobby);
            gameLobby.Dispose();
        }






        public void Dispose()
        {
            _serverUpTimer?.Dispose();
        }
    }
}
