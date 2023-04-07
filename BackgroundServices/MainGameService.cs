using System.Net.WebSockets;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using WebSocketsSample.Controllers;
using GameServer.Modules;

namespace GameServer.BackgroundServices
{
    public class MainGameService : IHostedService, IDisposable
    {
        //public static MainGameService Instance = new MainGameService();
        //private List<WebSocket> _sockets = new List<WebSocket>();
        private static int usingResource = 0;
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
                    if (true) //delete this after testing is finished. it causes problems if left undeleted
                    {
                        // create game session now and remove the lobby. MUST do this asap. getting to start the game session logic
                        GameSession gameSession = new GameSession(gameLobby.Clients);
                        gameSession.InitializeClients(gameSession);
                        await WebSocketController.BroadCastLobbyMessage(gameLobby, MessageType.MatchMakingSuccess);
                        TryCloseLobby(gameLobby);
                        _sessionHandlerService.AddGameSession(gameSession);
                    }
                }
                else
                {
                    GameLobby gameLobby = _lobbies.FirstOrDefault();
                    if (gameLobby != null)
                    {
                        gameClient.SetLobby(gameLobby);
                        gameLobby.AddClient(gameClient);
                        if (gameLobby.GetClientCount() >= 10) //hard coded user count must be changed into a variable type
                        {
                            // create game session now and remove the lobby. MUST do this asap. getting to start the game session logic
                            GameSession gameSession = new GameSession(gameLobby.Clients);
                            gameSession.InitializeClients(gameSession);
                            await WebSocketController.BroadCastLobbyMessage(gameLobby, MessageType.MatchMakingSuccess, "", 1000);
                            TryCloseLobby(gameLobby);
                            _sessionHandlerService.AddGameSession(gameSession);
                        }
                    }
                }
                _logger.LogInformation("New User Just Logged in!");
                socketFinishedTcs.SetResult(gameClient);
                Interlocked.Exchange(ref usingResource, 0);
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
                    gameLobby.RemoveClient(gameClient);
                    TryCloseLobby(gameLobby);
                    Interlocked.Exchange(ref usingResource, 0);
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
                    _lobbies.Remove(gameLobby);
                    gameLobby.Dispose();
                }
            }
        }

        public void Dispose()
        {
            _serverUpTimer?.Dispose();
        }
    }
}
