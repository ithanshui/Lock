using org.apache.zookeeper;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DotNetLock.ZK.Lock
{
    /// <summary>
    /// 功能描述：
    /// 创建人：yjq 2018/8/21 17:21:16
    /// </summary>
    public class ZkLockUtil
    {
        private static ZkLockOption _zkLockOption = null;
        private static readonly object _ObjLock = new object();
        private static List<ZkManager> _ZkMangerPool = new List<ZkManager>();
        private static int _currentReuqestCount = 0;

        public static ConcurrentDictionary<string, string> _KeyValueList = new ConcurrentDictionary<string, string>();

        public async static Task InstallAsync(int poolSize = 10)
        {
            for (int i = 0; i < poolSize; i++)
            {
                var zk = await CreateZkAsync();
                _ZkMangerPool.Add(zk);
            }
        }

        //替换成自己的实现
        public static ZkLockOption Option
        {
            get
            {
                if (_zkLockOption == null)
                {
                    _zkLockOption = new ZkLockOption { ConnectionString = "", LockRoot = "/Lock" };
                }
                return _zkLockOption;
            }
        }

        public async static Task<ZkManager> GetZkManagerAsync()
        {
            var zkManager = _ZkMangerPool[_currentReuqestCount % _ZkMangerPool.Count];
            Interlocked.Increment(ref _currentReuqestCount);
            if (zkManager != null && zkManager.ZK.getState() != ZooKeeper.States.CONNECTED)
            {
                await zkManager.CloseAsync();
                zkManager.Dispose();
                zkManager = await CreateZkAsync();
            }
            return zkManager;
        }

        public async static Task<ZkManager> CreateZkAsync()
        {
            ZkManager.LogToFile = false;
            var zk = new ZkManager(Option.GetZkOption(), ZkChangedAsync);
            var createRootTask = zk.ExistsAsync(Option.LockRoot);
            if (!createRootTask.Result)
            {
                await zk.CreateAsync(Option.LockRoot, null, CreateMode.PERSISTENT);
            }
            return zk;
        }

        private async static Task ZkChangedAsync(WatchedEvent @event, ZkManager zk)
        {
            if (@event.get_Type() == Watcher.Event.EventType.NodeDeleted)
            {
                GetAutoResetEvent(@event.getPath())?.Set();
            }
            await Task.Delay(1);
        }

        public async static Task<bool> GetLockAsync(string key, string value, TimeSpan span)
        {
            var zk = await GetZkManagerAsync();
            var nodeName = await zk.CreateAsync(GetFullNodeName(key), null, CreateMode.EPHEMERAL_SEQUENTIAL);

            var currentNodeName = nodeName.Substring(nodeName.LastIndexOf("/", StringComparison.Ordinal) + 1);

            _KeyValueList[currentNodeName] = value;

            var nodeList = await zk.GetChlidrenListAsync(Option.LockRoot);
            var currentNodeList = nodeList.Where(m => m.StartsWith(SetPrefix(key))).OrderBy(m => m).ToArray();
            if (currentNodeList != null && currentNodeList.Any() && currentNodeList[0] == currentNodeName)
            {
                return true;
            }
            else
            {
                var currentNodeIndex = Array.BinarySearch(currentNodeList, currentNodeName);
                var prevNodeName = currentNodeList[currentNodeIndex - 1];
                if (!(await zk.ExistsAsync(GetFullName(prevNodeName), watch: true)))
                {
                    var autoResetEvent = CreateAutoResetEvent(GetFullName(prevNodeName));
                    bool r = autoResetEvent.WaitOne(span);
                    DeleteAutoResetEvent(GetFullName(prevNodeName));
                    return r;
                }
                else
                {
                    return true;
                }
            }
        }

        public async static Task<bool> LockReleaseAsync(string key, string value)
        {
            var list = _KeyValueList.Where(m => m.Key.StartsWith(SetPrefix(key)) && m.Value == value);
            foreach (var item in list)
            {
                var zk = await GetZkManagerAsync();
                await zk.DeleteAsync(GetFullName(item.Key));
                if (_KeyValueList.ContainsKey(item.Key))
                {
                    _KeyValueList.TryRemove(item.Key, out string dicValue);
                }
            }
            return true;
        }

        public static string SetPrefix(string key)
        {
            return "lock_" + key + "_lock";
        }

        public static string GetFullNodeName(string key)
        {
            return GetFullName(SetPrefix(key));
        }

        public static string GetFullName(string currentName)
        {
            return Option.LockRoot + "/" + currentName;
        }

        private static ConcurrentDictionary<string, AutoResetEvent> autoResetEventDic = new ConcurrentDictionary<string, AutoResetEvent>();

        public static AutoResetEvent CreateAutoResetEvent(string nodePath)
        {
            var autoResetEvent = new AutoResetEvent(false);
            autoResetEventDic[nodePath] = autoResetEvent;
            return autoResetEvent;
        }

        public static AutoResetEvent GetAutoResetEvent(string nodePath)
        {
            if (autoResetEventDic.ContainsKey(nodePath))
            {
                return autoResetEventDic[nodePath];
            }
            return null;
        }

        public static void DeleteAutoResetEvent(string nodePath)
        {
            if (autoResetEventDic.ContainsKey(nodePath))
            {
                autoResetEventDic.TryRemove(nodePath, out AutoResetEvent value);
                value.Dispose();
                value = null;
            }
        }

        public static void Log()
        {
            Console.WriteLine(_KeyValueList.Count);
            Console.WriteLine(autoResetEventDic.Count);
        }
    }
}