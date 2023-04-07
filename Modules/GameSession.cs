using Microsoft.Win32.SafeHandles;
using System;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Mvc;
using WebSocketsSample.Controllers;
namespace GameServer.Modules
{
    public class GameSession : IDisposable
    {
        bool disposed = false;
        SafeHandle handle = new SafeFileHandle(IntPtr.Zero, true);

        List<GameClient> _clients;

        private bool _hasStarted;
        private int _sessionId;// TODO : needs implementation
        private int _sessionTime;

        public GameSession(List<GameClient> clients)
        {
            _clients = new List<GameClient>();
            _clients.AddRange(clients);
            
            _hasStarted = false;
            _sessionTime = 0;
        }
        
        public void InitializeClients(GameSession gameSession)
        {
            foreach (GameClient gameClient in _clients)
            {
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
        public int GetServerTime()
        {
            return _sessionTime;
        }


        public void StartSessionLogic()
        {
            _hasStarted = true;
        }


        public async Task SendNetworkFunctionCall(GameSession gameSession, string args)
        {
            await WebSocketController.BroadCastSessionMessage(gameSession, MessageType.NetworkFunctionCall, args);
        }


        public async Task SendSessionTimer(GameSession gameSession)
        {
            await WebSocketController.BroadCastSessionMessage(gameSession, MessageType.SessionTimerUpdate, "", gameSession.GetServerTime());
        }

        public void AddClient(GameClient gameClient)
        {
            if (!_clients.Contains(gameClient))
            {
                _clients.Add(gameClient);
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
    }
}
