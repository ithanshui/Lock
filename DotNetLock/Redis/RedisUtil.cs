using StackExchange.Redis;

namespace DotNetLock.Redis
{
    /// <summary>
    /// 功能描述：
    /// 创建人：yjq 2018/8/21 17:49:58
    /// </summary>
    public class RedisUtil
    {
        static RedisUtil()
        {
            var conn = CreateConnection("");//换成自己的redis连接
            Database = conn.GetDatabase();
        }

        /// <summary>
        /// 创建一个连接
        /// </summary>
        /// <param name="connection">连接字符信息</param>
        /// <returns></returns>
        private static ConnectionMultiplexer CreateConnection(string connection)
        {
            var conn = ConnectionMultiplexer.Connect(connection);

            return conn;
        }

        public static IDatabase Database { get; private set; }
    }
}