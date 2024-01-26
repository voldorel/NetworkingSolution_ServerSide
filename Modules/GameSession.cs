using Microsoft.Win32.SafeHandles;
using System;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using WebSocketsSample.Controllers;
namespace GameServer.Modules
{
    public class GameSession : IDisposable
    {
        bool disposed = false;
        SafeHandle handle = new SafeFileHandle(IntPtr.Zero, true);
        private static int usingResource = 0;
        List<GameClient> _clients;

        private bool _hasStarted;
        private int _sessionId;// TODO : needs implementation
        private int _sessionTime;
        private int _randomSeed;
        private List<GameEvent> _events;

        public GameSession(List<GameClient> clients, int randomSeed)
        {
            _clients = new List<GameClient>();
            _clients.AddRange(clients);

            _hasStarted = false;
            _sessionTime = 0;
            _events = new List<GameEvent>();
            _randomSeed = randomSeed; 
        }
        
        public void InitializeClients(GameSession gameSession)
        {
            foreach (GameClient gameClient in _clients)
            {
                gameClient.ClearAssignedLobby();
                gameClient.SetGameSession(gameSession);
            }
        }

        public void IncrementSessionTime()
        {
            Interlocked.Increment(ref _sessionTime);
        }

        public List<GameClient> GetGameClients()
        {
            return _clients;
        }
        public int GetSessionTime()
        {
            return _sessionTime;
        }


        public void StartSessionLogic()
        {
            _hasStarted = true;
        }


        public void AddGameEvent(string args, MessageType eventType, GameClient senderClient)
        {
            try
            {
                GameEvent gameEvent = new GameEvent(senderClient, eventType, _sessionTime, args);
                if (Interlocked.Exchange(ref usingResource, 1) == 0)
                {
                    _events.Add(gameEvent);
                    Interlocked.Exchange(ref usingResource, 0);
                }
            }
            catch
            {
                Console.WriteLine("adding game event failed");
            }
        }

        public async Task SendNetworkFunctionCall(GameSession gameSession, string args, GameClient senderClient)
        {
            if (gameSession == null) return;
            

            AddGameEvent(args, MessageType.NetworkFunctionCall, senderClient);
            
            await WebSocketController.BroadCastSessionMessage(gameSession, MessageType.NetworkFunctionCall, args);
        }


        public async Task SendSessionTimer(GameSession gameSession)
        {
            await WebSocketController.BroadCastSessionMessage(gameSession, MessageType.SessionTimerUpdate, "", null, gameSession.GetSessionTime());
        }

        public void AddClient(GameClient gameClient)
        {
            if (!_clients.Contains(gameClient))
            {
                _clients.Add(gameClient);
            }
        }


        public string GetMatchData()
        {
            JObject matchDataJObject = new JObject();
            JArray sessionMembers = new JArray();
            foreach (GameClient gameClient in _clients)
            {
                JObject jToken = new JObject();
                jToken["memberId"] = gameClient.GetUserId();
                sessionMembers.Add(jToken);
            }
            matchDataJObject["members"] = sessionMembers;
            matchDataJObject["randomSeed"] = _randomSeed;
            return matchDataJObject.ToString();
        }


        public async Task SendAllNetworkEvents(CancellationToken token, GameClient targetClient, int startingTime, int endingTime)
        {
            try
            {
                int eventCount = _events.Count;
                foreach (GameEvent gameEvent in _events)
                {
                    int eventTime = gameEvent.GetEventTime();
                    if (eventTime < startingTime || eventTime >= endingTime)
                        continue;
                    JObject keyValuePairs = new JObject();
                    keyValuePairs.Add("senderPlayer", gameEvent.GetSenderId());
                    keyValuePairs.Add("eventTime", gameEvent.GetEventTime());
                    keyValuePairs.Add("eventBody", gameEvent.GetEventBody());
                    //Console.WriteLine(eventTime + " " + startingTime + " " + endingTime + " " + gameEvent.GetEventType());
                    MessageType messageType = gameEvent.GetEventType();
                    switch (messageType)
                    {
                        default:
                        case MessageType.NetworkFunctionCall:
                            messageType = MessageType.PreSyncNetworkFunctionCall;
                            break;
                        case MessageType.PlayerEnteredSession:
                            messageType = MessageType.PreSyncPlayerEntered;
                            break;
                        case MessageType.PlayerLeftSession:
                            messageType = MessageType.PreSyncPlayerLeft;
                            break;
                    }
                    await WebSocketController.SendSingleSessionMessage(messageType, keyValuePairs.ToString(), targetClient);
                }
                await WebSocketController.SendSingleSessionMessage(MessageType.SyncTransferFinished, "", targetClient);
            } catch
            {
                Console.WriteLine("Send game sync data failed");
            }
        }



        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                handle.Dispose();
                _clients.Clear();
            }

            disposed = true;
        }



        private class GameEvent
        {
            public GameEvent(GameClient gameClient, MessageType eventType, int eventTime, string eventMessage)
            {
                _senderPlayer = gameClient;
                _eventTime = eventTime;
                _message = eventMessage;
                _eventType = eventType;
            }
            private GameClient _senderPlayer;
            private int _eventTime;
            private string _message;
            private MessageType _eventType;
            public int GetEventTime()
            {
                return _eventTime;
            }
            public string GetEventBody()
            {
                return _message;
            }

            public MessageType GetEventType()
            {
                return _eventType;
            }
            
            public string GetSenderId() //gonna send username for now but needs to be swapped with user id 
            {
                return _senderPlayer.GetUsername();
            }
        }
    }
}
