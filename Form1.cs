namespace MySocketServer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 取 <see cref="tx_sendMessage"></see> 的值
        /// </summary>
        public string Message => tx_sendMessage.Text.Trim();

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
    }
}