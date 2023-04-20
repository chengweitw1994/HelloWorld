using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace MySocketServer.Domain
{
    public class SocketSvr : IDisposable
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private TcpListener? _tcpListener = null;
        private ConcurrentBag<Task> _tasks = new ConcurrentBag<Task>();
        private IPAddress _ipAddress;
        public string IP { get; private set; }
        public int Port { get; private set; }

        public SocketSvr(string ip, int port)
        {
            IP = ip;
            Port = port;
            _ipAddress = IPAddress.Parse(IP);
        }

        public void Start()
        {
            // TODO: if tcpListener is open then close it.
            _tcpListener = new TcpListener(_ipAddress, Port);
            _tcpListener.Start();
        }

        public async Task Shutdown()
        {
            _cancellationTokenSource.Cancel();
            await Task.WhenAll(_tasks.ToArray());
        }

        #region Implement IDisposable
        private bool _disposed = false;

        /// <summary>
        /// Public implementation of Dispose pattern callable by consumers.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                _disposed = true;
            }
        }

        ~SocketSvr()
        {
            Dispose(false);
        }
        #endregion
    }
}
