using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace MySocketServer.Domain
{
    public class SocketSvr : IDisposable
    {
        private ServerStateEnum _state = ServerStateEnum.OutOfService;
        private readonly IPAddress _ipAddress;
        private CancellationTokenSource? _cancellationTokenSource;
        private ConcurrentBag<Task>? _tasks;
        private TcpListener? _tcpListener;
        private ConcurrentBag<TcpClient> _tcpClients;
        public string IP { get; private set; }
        public int Port { get; private set; }

        public SocketSvr(string ip, int port)
        {
            IP = ip;
            Port = port;
            _ipAddress = IPAddress.Parse(IP);
        }

        private bool _canStart => _state == ServerStateEnum.OutOfService;

        /// <summary>
        /// 啟動
        /// </summary>
        public void Start()
        {
            if (!_canStart) return;

            try
            {
                _state = ServerStateEnum.OnStarting;

                _cancellationTokenSource = new CancellationTokenSource();
                _tasks = new ConcurrentBag<Task>();

                #region background task
                _tasks.Add(
                    Task.Run(() =>
                    DoSomething("背景程式 A", _cancellationTokenSource.Token), _cancellationTokenSource.Token));
                #endregion

                // TODO: if tcpListener is open then close it.
                _tcpListener = new TcpListener(_ipAddress, Port);
                _tcpListener.Start();

                StartedSuccessfully();

                while (true)
                {
                    Console.Write("Waiting for a connection... ");

                    // Perform a blocking call to accept requests.
                    // You could also use server.AcceptSocket() here.
                    TcpClient tcpClient = _tcpListener.AcceptTcpClient();
                    Console.WriteLine("Connected!");
                    _tcpClients.Add(tcpClient);

                    Task.Run(() => MyHandler(tcpClient));
                }
            }
            catch (Exception ex)
            {
                StartFailed();
            }
        }

        /// <summary>
        /// 啟動失敗
        /// </summary>
        private void StartFailed()
        {
            _state = ServerStateEnum.OutOfService;
        }

        /// <summary>
        /// 服務已成功啟動
        /// </summary>
        private void StartedSuccessfully()
        {
            _state = ServerStateEnum.OnService;
        }

        private bool _canShutdown => _state == ServerStateEnum.OnService;

        /// <summary>
        /// 停止
        /// </summary>
        /// <returns></returns>
        public async Task Shutdown()
        {
            if (!_canShutdown) return;

            try
            {
                _state = ServerStateEnum.OnClosing;

                _tcpListener?.Stop();

                _cancellationTokenSource?.Cancel();

                if (_tasks is not null)
                    await Task.WhenAll(_tasks.ToArray());

                ShutdownSuccessfully();
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine($"\n{nameof(OperationCanceledException)} thrown\n");

                // TODO:
                ShutdownSuccessfully();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n{nameof(Exception)} thrown {ex}\n");

                ShutdownError();
            }
            finally
            {
                _cancellationTokenSource?.Dispose();
            }

            // Display status of all tasks.
            if (_tasks is not null)
            {
                foreach (var task in _tasks)
                    Console.WriteLine("Task {0} status is now {1}", task.Id, task.Status);

                _tasks?.Clear();
            }
        }

        /// <summary>
        /// 停止時發生錯誤
        /// </summary>
        private void ShutdownError()
        {
            _state = ServerStateEnum.Error;
        }

        /// <summary>
        /// 服務已成功停止
        /// </summary>
        private void ShutdownSuccessfully()
        {
            _state = ServerStateEnum.OutOfService;
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

        private async Task DoSomething(string taskName, CancellationToken ct)
        {
            // Was cancellation already requested?
            if (ct.IsCancellationRequested)
            {
                Console.WriteLine("Task {0} was cancelled before it got started.",
                                  taskName);
                ct.ThrowIfCancellationRequested();
            }

            while (true)
            {
                await Task.Delay(1000, ct);

                Console.WriteLine("Task {0} 工作中...", taskName);

                if (ct.IsCancellationRequested)
                {
                    Console.WriteLine("Task {0} 已取消", taskName);
                    ct.ThrowIfCancellationRequested();
                }
            }
        }

        private void MyHandler(TcpClient tcpClient)
        {
            // Buffer for reading data
            Byte[] bytes = new Byte[256];
            string data;
            int i;

            // Get a stream object for reading and writing
            using NetworkStream stream = tcpClient.GetStream();

            // Loop to receive all the data sent by the client.
            while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
            {
                // Translate data bytes to a ASCII string.
                data = System.Text.Encoding.UTF8.GetString(bytes, 0, i);
                Console.WriteLine("[Received]: {0}", data);

                byte[] msg = System.Text.Encoding.UTF8.GetBytes(string.Format("我是Server, 我聽到你說: {0}", data));

                // Send back a response.
                stream.Write(msg, 0, msg.Length);
                Console.WriteLine("[Sent]: {0}", data);
            }
        }
    }
}
