using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace DotNetLock.Redis
{
    /// <summary>
    /// 功能描述：
    /// 创建人：yjq 2018/8/21 18:00:16
    /// </summary>
    public class DefaultRedisLock : ILock
    {
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
                    var result = executeAction();
                    return result;
                }
                catch
                {
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
                }
                finally
                {
                    LockRelease(key, value);
                }
            }
            return defaultValue;
        }

        private static ConcurrentDictionary<string, string> _LockValueCache = new ConcurrentDictionary<string, string>();

        /// <summary>
        /// 释放一个锁
        /// </summary>
        /// <param name="key">锁的键</param>
        /// <param name="value">当前占用值</param>
        /// <returns>成功返回true</returns>
        public bool LockRelease(string key, string value)
        {
            if (_LockValueCache.ContainsKey(key))
            {
                if (_LockValueCache[key] == value)
                {
                    _LockValueCache.TryRemove(key, out string keyValue);
                    return RedisUtil.Database.LockRelease(key, value);
                }
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// 异步释放一个锁
        /// </summary>
        /// <param name="key">锁的键</param>
        /// <param name="value">当前占用值</param>
        /// <returns>成功返回true</returns>
        public Task<bool> LockReleaseAsync(string key, string value)
        {
            return Task.FromResult(LockRelease(key, value));
        }

        /// <summary>
        /// 获取一个锁(需要自己释放)
        /// </summary>
        /// <param name="key">锁的键</param>
        /// <param name="value">当前占用值</param>
        /// <param name="span">耗时时间</param>
        /// <returns>成功返回true</returns>
        public bool LockTake(string key, string value, TimeSpan span)
        {
            var isSuccess = GetLock(key, value, span);
            if (isSuccess)
            {
                _LockValueCache[key] = value;
            }
            return isSuccess;
        }

        /// <summary>
        ///  异步获取一个锁(需要自己释放)
        /// </summary>
        /// <param name="key">锁的键</param>
        /// <param name="value">当前占用值</param>
        /// <param name="span">耗时时间</param>
        /// <returns>成功返回true</returns>
        public Task<bool> LockTakeAsync(string key, string value, TimeSpan span)
        {
            return Task.FromResult(LockTake(key, value, span));
        }

        private bool GetLock(string key, string value, TimeSpan span)
        {
            var cts = new CancellationTokenSource(span);
            var ct = cts.Token;
            var task = Task.Factory.StartNew(() =>
            {
                while (!RedisUtil.Database.LockTake(key, value, span))
                {
                    Task.Delay(1);
                }
                return true;
            }, ct);
            try
            {
                task.Wait();
            }
            catch
            {
                if (task.IsCanceled)
                {
                    return false;
                }
            }
            if (task.IsCanceled)
            {
                return false;
            }
            return task.Result;
        }
    }
}