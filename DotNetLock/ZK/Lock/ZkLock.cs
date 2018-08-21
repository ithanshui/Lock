using System;
using System.Threading.Tasks;

namespace DotNetLock.ZK.Lock
{
    /// <summary>
    /// 功能描述：
    /// 创建人：yjq 2018/8/21 17:23:43
    /// </summary>
    public class ZkLock : ILock
    {
        public bool LockTake(string key, string value, TimeSpan span)
        {
            var task = LockTakeAsync(key, value, span);
            task.Wait();
            return task.Result;
        }

        public Task<bool> LockTakeAsync(string key, string value, TimeSpan span)
        {
            return ZkLockUtil.GetLockAsync(key, value, span);
        }

        public bool LockRelease(string key, string value)
        {
            var task = LockReleaseAsync(key, value);
            return task.Result;
        }

        public Task<bool> LockReleaseAsync(string key, string value)
        {
            return ZkLockUtil.LockReleaseAsync(key, value);
        }

        /// <summary>
        /// 使用锁执行一个方法
        /// </summary>
        /// <param name="key">锁的键</param>
        /// <param name="value">当前占用值</param>
        /// <param name="span">耗时时间</param>
        /// <param name="executeAction">要执行的方法</param>
        public void ExecuteWithLock(string key, string value, TimeSpan span, Action executeAction)
        {
            if (executeAction == null) return;

            if (LockTake(key, value, span))
            {
                try
                {
                    executeAction();
                }
                finally
                {
                    LockRelease(key, value);
                }
            }
        }

        /// <summary>
        /// 使用锁执行一个方法
        /// </summary>
        /// <typeparam name="T">返回值类型</typeparam>
        /// <param name="key">锁的键</param>
        /// <param name="value">当前占用值</param>
        /// <param name="span">耗时时间</param>
        /// <param name="executeAction">要执行的方法</param>
        /// <param name="defaultValue">默认返回</param>
        /// <returns></returns>
        public T ExecuteWithLock<T>(string key, string value, TimeSpan span, Func<T> executeAction, T defaultValue = default(T))
        {
            if (executeAction == null) return defaultValue;

            if (LockTake(key, value, span))
            {
                try
                {
                    return executeAction();
                }
                finally
                {
                    LockRelease(key, value);
                }
            }
            return defaultValue;
        }

        /// <summary>
        /// 使用锁执行一个异步方法
        /// </summary>
        /// <param name="key">锁的键</param>
        /// <param name="value">当前占用值</param>
        /// <param name="span">耗时时间</param>
        /// <param name="executeAction">要执行的方法</param>
        public async Task ExecuteWithLockAsync(string key, string value, TimeSpan span, Func<Task> executeAction)
        {
            if (executeAction == null) return;

            if (await LockTakeAsync(key, value, span))
            {
                try
                {
                    await executeAction();
                }
                catch
                {
                    throw;
                }
                finally
                {
                    LockRelease(key, value);
                }
            }
        }

        /// <summary>
        /// 使用锁执行一个异步方法
        /// </summary>
        /// <typeparam name="T">返回值类型</typeparam>
        /// <param name="key">锁的键</param>
        /// <param name="value">当前占用值</param>
        /// <param name="span">耗时时间</param>
        /// <param name="executeAction">要执行的方法</param>
        /// <param name="defaultValue">默认返回</param>
        /// <returns></returns>
        public async Task<T> ExecuteWithLockAsync<T>(string key, string value, TimeSpan span, Func<Task<T>> executeAction, T defaultValue = default(T))
        {
            if (executeAction == null) return defaultValue;
            if (await LockTakeAsync(key, value, span))
            {
                try
                {
                    return await executeAction();
                }
                catch
                {
                    throw;
                }
                finally
                {
                    LockRelease(key, value);
                }
            }
            return defaultValue;
        }
    }
}