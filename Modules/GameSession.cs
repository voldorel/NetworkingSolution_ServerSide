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
        private List<GameEvent> _events;

        public GameSession(List<GameClient> clients)
        {
            _clients = new List<GameClient>();
            _clients.AddRange(clients);

            _hasStarted = false;
            _sessionTime = 0;
            _events = new List<GameEvent>();
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


        public async Task SendNetworkFunctionCall(GameSession gameSession, string args, GameClient senderClient)
        {
            if (gameSession == null) return;
            

            try
            {
                GameEvent gameEvent = new GameEvent(senderClient, _sessionTime, args);
                if (Interlocked.Exchange(ref usingResource, 1) == 0)
                {
                    _events.Add(gameEvent);
                    Interlocked.Exchange(ref usingResource, 0);
                }
            } catch
            {

            }
            
            await WebSocketController.BroadCastSessionMessage(gameSession, MessageType.NetworkFunctionCall, args);
        }


        public async Task SendSessionTimer(GameSession gameSession)
        {
            await WebSocketController.BroadCastSessionMessage(gameSession, MessageType.SessionTimerUpdate, "", gameSession.GetSessionTime());
        }

        public void AddClient(GameClient gameClient)
        {
            if (!_clients.Contains(gameClient))
            {
                _clients.Add(gameClient);
            }
        }



        public async Task SendAllNetworkEvents(CancellationToken token, GameClient targetClient, int startingTime, int endingTime)
        {
            try
            {
                int eventCount = _events.Count;
                foreach (GameEvent gameEvent in _events)
                {
                    int eventTime = gameEvent.GetEventTime();
                    if (eventTime <= startingTime || eventTime > endingTime)
                        continue;
                    JObject keyValuePairs = new JObject();
                    keyValuePairs.Add("senderPlayer", gameEvent.GetSenderId());
                    keyValuePairs.Add("eventTime", gameEvent.GetEventTime());
                    keyValuePairs.Add("eventBody", gameEvent.GetEventBody());
                    await WebSocketController.SendSingleSessionMessage(MessageType.PreSyncNetworkFunctionCall, keyValuePairs.ToString(), targetClient);
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
            public GameEvent(GameClient gameClient, int eventTime, string eventMessage)
            {
                _senderPlayer = gameClient;
                _eventTime = eventTime;
                _message = eventMessage;
            }
            private GameClient _senderPlayer;
            private int _eventTime;
            private string _message;
            public int GetEventTime()
            {
                return _eventTime;
            }
            public string GetEventBody()
            {
                return _message;
            }
            
            public string GetSenderId() //gonna send username for now but needs to be swapped with user id 
            {
                return _senderPlayer.GetUsername();
            }
        }
    }
}
