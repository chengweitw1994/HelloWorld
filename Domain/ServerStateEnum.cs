namespace MySocketServer.Domain
{
    /// <summary>
    /// Server 狀態
    /// </summary>
    public enum ServerStateEnum
    {
        /// <summary>
        /// 正在啟動
        /// </summary>
        OnStarting,

        /// <summary>
        /// 服務中
        /// </summary>
        OnService,

        /// <summary>
        /// 正在關閉
        /// </summary>
        OnClosing,

        /// <summary>
        /// 已停止服務
        /// </summary>
        OutOfService,

        /// <summary>
        /// 發生未預期的錯誤
        /// </summary>
        Error
    }
}
