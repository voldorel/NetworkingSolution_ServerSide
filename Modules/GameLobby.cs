using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
namespace GameServer.Modules
{
    public class GameLobby : IDisposable
    {
        bool disposed = false;
        SafeHandle handle = new SafeFileHandle(IntPtr.Zero, true);

        //made public for faster access
        public List<GameClient> Clients = new List<GameClient>();

        public GameLobby(GameClient client)
        {
            Clients.Add(client);
        }

        public void AddClient(GameClient gameClient)
        {
            if (!Clients.Contains(gameClient))
            {
                Clients.Add(gameClient);
            }
        }

        public int GetClientCount()
        {
            return Clients.Count;
        }


        public void RemoveClient(GameClient gameClient)
        {
            if (Clients.Contains(gameClient))
            {
                Clients.Remove(gameClient);
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
                Clients.Clear();
            }

            disposed = true;
        }
    }
}
