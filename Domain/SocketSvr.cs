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
        private ConcurrentBag<Task>? _taskList;
        private TcpListener? _tcpListener;
        private ConcurrentBag<TcpClient>? _tcpClientList;

        /// <summary>
        /// Server 監聽的 IP 位址
        /// </summary>
        public string IP { get; private set; }

        /// <summary>
        /// Server 監聽的埠號
        /// </summary>
        public int Port { get; private set; }

        public SocketSvr(string ip, int port)
        {
            IP = ip;
            Port = port;
            _ipAddress = IPAddress.Parse(IP);
        }

        /// <summary>
        /// 是否可啟動
        /// </summary>
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
                _taskList = new ConcurrentBag<Task>();

                #region background task
                _taskList.Add(
                    Task.Run(() =>
                    DoSomething("背景程式 A", _cancellationTokenSource.Token), _cancellationTokenSource.Token));
                #endregion

                _tcpClientList = new ConcurrentBag<TcpClient>();

                // TODO: if tcpListener is open then close it.
                _tcpListener = new TcpListener(_ipAddress, Port);
                _tcpListener.Start();

                StartedSuccessfully();

                Task.Run(() => AcceptIncomingTcpClient(_cancellationTokenSource.Token), _cancellationTokenSource.Token);
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

                if (_taskList is not null)
                    await Task.WhenAll(_taskList.ToArray());

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
            if (_taskList is not null)
            {
                foreach (var task in _taskList)
                    Console.WriteLine("Task {0} status is now {1}", task.Id, task.Status);

                _taskList.Clear();
            }

            if (_tcpClientList is not null)
            {
                _tcpClientList.Clear();
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

        /// <summary>
        /// 模擬持續作業的背景程式
        /// </summary>
        /// <param name="taskName"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
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

        private async Task AcceptIncomingTcpClient(CancellationToken ct)
        {
            // Was cancellation already requested?
            if (ct.IsCancellationRequested)
            {
                Console.WriteLine("Task {0} was cancelled before it got started.",
                                  nameof(AcceptIncomingTcpClient));
                ct.ThrowIfCancellationRequested();
            }

            while (true)
            {
                if (ct.IsCancellationRequested)
                {
                    Console.WriteLine("Task {0} 已取消", nameof(AcceptIncomingTcpClient));
                    ct.ThrowIfCancellationRequested();
                }
                if (_tcpListener is null) break;
                if (_tcpClientList is null) break;

                Console.WriteLine("Task {0} 工作中...", nameof(AcceptIncomingTcpClient));

                Console.WriteLine("Waiting for a connection... ");

                var tcpClient = await _tcpListener.AcceptTcpClientAsync(ct);
                Console.WriteLine("Connected!");
                _tcpClientList.Add(tcpClient);

                _ = Task.Run(() => MyHandler(tcpClient, ct), ct);
            }
        }

        private async Task MyHandler(TcpClient tcpClient, CancellationToken ct)
        {
            // Buffer for reading data
            Byte[] bytes = new Byte[256];
            string data;
            int i;

            // Get a stream object for reading and writing
            using NetworkStream stream = tcpClient.GetStream();

            // Loop to receive all the data sent by the client.
            while ((i = await stream.ReadAsync(bytes, 0, bytes.Length, ct)) != 0)
            {
                // Translate data bytes to a UTF8 string.
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
