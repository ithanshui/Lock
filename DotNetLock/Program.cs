using DotNetLock.Redis;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DotNetLock
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            #region 本地缓存锁

            //ILock localLock = new LocalLock();

            #endregion 本地缓存锁

            #region zk锁
            //ZkLockUtil Option需要设置自己的zk的连接地址
            //ZkLockUtil.InstallAsync().Wait();
            //Console.WriteLine("开始执行");
            //ILock localLock = new ZkLock();

            #endregion zk锁

            //RedisUtil 类中需要设置自己的redis连接地址
            ILock localLock = new DefaultRedisLock();

            int excuteCount = 0;
            Parallel.For(0, 100, i =>
            {
                localLock.ExecuteWithLock("test", Guid.NewGuid().ToString(), TimeSpan.FromSeconds(5), () =>
                {
                    Console.WriteLine("获取锁成功");
                    Interlocked.Increment(ref excuteCount);
                });
            });
            Console.WriteLine("成功次数:" + excuteCount.ToString());
            Console.WriteLine("执行完成");
            Console.ReadLine();
        }
    }
}