namespace DotNetLock.ZK.Lock
{
    /// <summary>
    /// 功能描述：
    /// 创建人：yjq 2018/8/21 17:20:46
    /// </summary>
    public class ZkLockOption : ZkOption
    {
        /// <summary>
        /// 锁目录的父级路径
        /// </summary>
        public string LockRoot { get; set; }

        public ZkOption GetZkOption()
        {
            return new ZkOption
            {
                ConnectionString = ConnectionString,
                ConnectionTimeout = ConnectionTimeout,
                OperatingTimeout = OperatingTimeout,
                ReadOnly = ReadOnly,
                RetryCount = RetryCount,
                SessionId = SessionId,
                SessionPasswd = SessionPasswd,
                SessionTimeout = SessionTimeout
            };
        }
    }
}