using GameServer.Modules;
namespace GameServer.BackgroundServices
{
    //public interface IGameSessionHandlerService
    //{
    //    ValueTask GameSessionHandlerService(Func<CancellationToken, ValueTask> workItem);

    //    ValueTask<Func<CancellationToken, ValueTask>> DequeueAsync(
    //        CancellationToken cancellationToken);
    //}

    public class GameSessionHandlerService : BackgroundService
    {
        private readonly ILogger<GameSessionHandlerService> _logger;
        private static int usingResource = 0;
        private int _executionCount;
        private List<GameSession> _gameSessions;
        private readonly float _serverTickRateFrequency = 50f;
        private readonly IBackgroundTaskQueue _taskQueue;
        private readonly CancellationToken _cancellationToken;



        public GameSessionHandlerService(ILogger<GameSessionHandlerService> logger, IConfiguration configuration, IHostApplicationLifetime applicationLifetime, IBackgroundTaskQueue taskQueue)
        {
            _logger = logger;
            try
            {
                if (ushort.TryParse(configuration["App:TickRate"], out var ct))
                {
                    _serverTickRateFrequency = 1000f/ct;
                }
            } catch { }
            _logger.LogInformation("Game Session Service running.");
            _gameSessions = new List<GameSession>();
            _cancellationToken = applicationLifetime.ApplicationStopping;
            _taskQueue = taskQueue;
        }



        public void AddGameSession(GameSession gameSession)
        {
            if (Interlocked.Exchange(ref usingResource, 1) == 0)
            {
                if (_gameSessions == null)
                {
                    _gameSessions = new List<GameSession>();
                }
                if (!_gameSessions.Contains(gameSession))
                {
                    _gameSessions.Add(gameSession);
                }
                Interlocked.Exchange(ref usingResource, 0);
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            // When the timer should have no due-time, then do the work once now.
            DoWork();

            using PeriodicTimer timer = new(TimeSpan.FromMilliseconds(_serverTickRateFrequency));

            try
            {
                while (await timer.WaitForNextTickAsync(stoppingToken))
                {
                    DoWork();
                    if (_gameSessions.Count > 0)
                        await _taskQueue.QueueBackgroundWorkItemAsync(SessionTimerTask);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Game Session Service is stopping.");
            }
        }

        // Could also be a async method, that can be awaited in ExecuteAsync above
        private void DoWork()
        {
            int count = Interlocked.Increment(ref _executionCount);
            foreach (GameSession gameSession in _gameSessions)
            {
                gameSession.IncrementSessionTime();

                //_logger.LogInformation(gameSession.GetServerTime() + "");
            }
            //_logger.LogInformation("Timed Hosted Service is working. Count: {Count}", count);
        }

        public async Task DoNetworkFunctionCall(GameSession gameSession, string args, GameClient senderClient, GameClient targetClient = null)
        {
            if (_gameSessions.Count > 0)
            {
                Func<CancellationToken, ValueTask> networkTask = async (cancellationToken) => {
                    await DoNetworkFunctionCallTask(cancellationToken, args, senderClient);
                };
                await _taskQueue.QueueBackgroundWorkItemAsync(networkTask);
            }
        }

        private async ValueTask DoNetworkFunctionCallTask(CancellationToken token, string args, GameSession targetSession, GameClient senderClient, GameClient targetClient = null)
        {
            if (targetClient == null)
            {
                await targetSession.SendNetworkFunctionCall(targetSession, args, senderClient, targetClient);
            } else
            {
                await targetSession.SendNetworkFunctionCall(targetSession, args, senderClient);
            }
        }



        private async ValueTask SessionTimerTask( CancellationToken token)
        {


            foreach (GameSession gameSession in _gameSessions)
            {
                await gameSession.SendSessionTimer(gameSession);
            }
        }
    }
}
