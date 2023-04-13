using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace MySocketServer
{
    public partial class Form1 : Form
    {
        private static CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private static TcpListener? server = null;
        private static ConcurrentBag<Task> tasks = new ConcurrentBag<Task>();

        public Form1()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 取 <see cref="tx_sendMessage"></see> 的值
        /// </summary>
        public string Message => tx_sendMessage.Text.Trim();

        /// <summary>
        /// Server IP
        /// </summary>
        public string ServerIp { get; set; } = "127.0.0.1";

        /// <summary>
        /// Server Port
        /// </summary>
        public int ServerPort { get; set; } = 80;

        private void btn_sendMessage_Click(object sender, EventArgs e)
        {
            Console.WriteLine(ConsoleMessageTemplate(Message));
        }

        /// <summary>
        /// 清空 Console 視窗的文字
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_clearConsole_Click(object sender, EventArgs e)
        {
            Console.Clear();
        }

        private static string ConsoleMessageTemplate(string message)
        {
            return string.Format("{0} | {1}"
                , DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fffffff")
                , message);
        }

        /// <summary>
        /// 啟動 Server
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_startServer_Click(object sender, EventArgs e)
        {
            string message = "嘗試啟動Server";
            Console.WriteLine(ConsoleMessageTemplate(message));

            if (server is not null) return;

            try
            {
                IPAddress ipAddress = IPAddress.Parse(ServerIp);

                //server = new TcpListener(ipAddress, ServerPort);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ConsoleMessageTemplate(ex.Message));
            }
        }

        /// <summary>
        /// 關閉 Server
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_shutdownServer_Click(object sender, EventArgs e)
        {
            string message = "嘗試關閉Server";
            Console.WriteLine(ConsoleMessageTemplate(message));
        }
    }
}