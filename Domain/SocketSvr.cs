using System.Collections.Concurrent;
using System.Net;

namespace MySocketServer.Domain
{
    public class SocketSvr : IDisposable
    {
        private ServerStateEnum _state = ServerStateEnum.OutOfService;
        private readonly IPAddress _ipAddress;
        private CancellationTokenSource? _cancellationTokenSource;
        private ConcurrentBag<Task>? _tasks;
        //private TcpListener? _tcpListener;
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
                Task task;
                task = Task.Run(() => DoSomething("背景程式1", _cancellationTokenSource.Token), _cancellationTokenSource.Token);
                _tasks.Add(task);

                task = Task.Run(() =>
                {
                    // Create some cancelable child tasks.
                    Task tc;
                    for (int i = 3; i <= 10; i++)
                    {
                        // For each child task, pass the same token
                        // to each user delegate and to Task.Run.
                        tc = Task.Run(() => DoSomething($"背景程式{i}", _cancellationTokenSource.Token), _cancellationTokenSource.Token);
                        Console.WriteLine("Task {0} executing", tc.Id);
                        _tasks.Add(tc);
                        // Pass the same token again to do work on the parent task.
                        // All will be signaled by the call to tokenSource.Cancel below.
                        DoSomething("背景程式2", _cancellationTokenSource.Token);
                    }
                }, _cancellationTokenSource.Token);
                _tasks.Add(task);
                #endregion

                // TODO: if tcpListener is open then close it.
                //_tcpListener = new TcpListener(_ipAddress, Port);
                //_tcpListener.Start();

                StartedSuccessfully();
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

                //_tcpListener?.Stop();

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

        static void DoSomething(string taskName, CancellationToken ct)
        {
            // Was cancellation already requested?
            if (ct.IsCancellationRequested)
            {
                Console.WriteLine("Task {0} was cancelled before it got started.",
                                  taskName);
                ct.ThrowIfCancellationRequested();
            }

            int maxIterations = 100;

            // NOTE!!! A "TaskCanceledException was unhandled
            // by user code" error will be raised here if "Just My Code"
            // is enabled on your computer. On Express editions JMC is
            // enabled and cannot be disabled. The exception is benign.
            // Just press F5 to continue executing your code.
            for (int i = 0; i <= maxIterations; i++)
            {
                Random random = new Random(Guid.NewGuid().GetHashCode());
                int bombNumber = random.Next(0, 100);

                // Do a bit of work. Not too much.
                var sw = new SpinWait();
                for (int j = 0; j <= 100; j++)
                {

                    if (j == bombNumber)
                    {
                        string bombMessage = $"Task {taskName} bomb at {i} {bombNumber}";
                        Console.WriteLine(bombMessage);

                        //throw new Exception(bombMessage);
                    }

                    Console.WriteLine("Task {0} do something {1} {2}", taskName, i, j);
                    sw.SpinOnce();

                    if (ct.IsCancellationRequested)
                    {
                        Console.WriteLine("Task {0} cancelled", taskName);
                        ct.ThrowIfCancellationRequested();
                    }
                }


                if (ct.IsCancellationRequested)
                {
                    Console.WriteLine("Task {0} cancelled", taskName);
                    ct.ThrowIfCancellationRequested();
                }
            }
        }
    }
}
