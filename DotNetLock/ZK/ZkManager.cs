using org.apache.zookeeper;
using org.apache.zookeeper.data;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DotNetLock.ZK
{
    /// <summary>
    /// 功能描述：
    /// 创建人：yjq 2018/8/21 17:17:59
    /// </summary>
    public class ZkManager : SelfDisposable
    {
        private readonly Func<WatchedEvent, ZkManager, Task> _changeEvent;

        public ZkManager(ZkOption zkOption, Func<WatchedEvent, ZkManager, Task> changeEvent)
        {
            Option = zkOption;
            _changeEvent = changeEvent;
            ZK = CreateZk();
        }

        public ZkOption Option { get; }

        private ZooKeeper CreateZk()
        {
            var zk = new ZooKeeper(Option.ConnectionString, Option.SessionTimeout, new ZkWatcher(_changeEvent, this), Option.SessionId, Option.SessionPasswdBytes, Option.ReadOnly);
            int currentTryCount = 0;
            while (zk.getState() != ZooKeeper.States.CONNECTED && currentTryCount < Option.RetryCount)
            {
                Thread.Sleep(1000);
            }
            return zk;
        }

        public ZooKeeper ZK { get; private set; }

        public static bool LogToFile
        {
            get
            {
                return ZooKeeper.LogToFile;
            }
            set
            {
                ZooKeeper.LogToFile = value;
            }
        }

        public async Task<bool> ExistsAsync(string path, bool watch = false)
        {
            var state = await ZK.existsAsync(path, watch: watch);
            return state != null;
        }

        public Task<string> CreateAsync(string path, byte[] data)
        {
            return CreateAsync(path, data, ZooDefs.Ids.OPEN_ACL_UNSAFE, CreateMode.EPHEMERAL);
        }

        public Task<string> CreateAsync(string path, byte[] data, CreateMode createMode)
        {
            return CreateAsync(path, data, ZooDefs.Ids.OPEN_ACL_UNSAFE, createMode);
        }

        public async Task<string> CreateAsync(string path, byte[] data, List<ACL> acl, CreateMode createMode)
        {
            string currentPath = string.Empty;
            var lastCreatePath = string.Empty;
            var pathList = path.Split('/');
            for (int i = 0; i < pathList.Length; i++)
            {
                if (!currentPath.EndsWith('/'))
                {
                    currentPath += "/";
                }
                currentPath += pathList[i];
                if (i > 0 && i < pathList.Length)
                {
                    var isExists = await ExistsAsync(currentPath);
                    if (!isExists)
                    {
                        var createdPath = await ZK.createAsync(currentPath, path == currentPath ? data : null, acl, createMode);
                        if (!string.IsNullOrWhiteSpace(createdPath))
                        {
                            lastCreatePath = createdPath;
                        }
                    }
                }
            }

            return lastCreatePath;
        }

        public async Task<bool> DeleteAsync(string sourcePath)
        {
            var exists = await ExistsAsync(sourcePath);
            if (exists)
            {
                await ZK.deleteAsync(sourcePath);
            }
            return true;
        }

        public async Task<List<string>> GetChlidrenListAsync(string sourcePath, Watcher watcher)
        {
            var isExists = await ExistsAsync(sourcePath);
            if (!isExists)
            {
                return new List<string>();
            }
            var childrenResult = await ZK.getChildrenAsync(sourcePath, watcher);
            return childrenResult.Children;
        }

        public async Task<List<string>> GetChlidrenListAsync(string sourcePath, bool watch = false)
        {
            var isExists = await ExistsAsync(sourcePath, watch: watch);
            if (!isExists)
            {
                return new List<string>();
            }
            var childrenResult = await ZK.getChildrenAsync(sourcePath, watch: watch);
            return childrenResult.Children;
        }

        public async Task<byte[]> GetDataAsync(string sourcePath, bool watch = false)
        {
            var exists = await ExistsAsync(sourcePath);
            if (exists)
            {
                var dataResult = await ZK.getDataAsync(sourcePath, watch: watch);
                return dataResult.Data;
            }
            return null;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task SetDataAsync(string sourcePath, byte[] data)
        {
            var exists = await ExistsAsync(sourcePath);
            if (exists)
            {
                await ZK.setDataAsync(sourcePath, data);
            }
        }

        public async Task ReConnectAsync()
        {
            if (!Monitor.TryEnter(this, Option.ConnectionTimeout))
                return;
            try
            {
                if (ZK != null)
                {
                    try
                    {
                        await CloseAsync();
                    }
                    catch
                    {
                        // ignored
                    }
                }
                ZK = CreateZk();
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        public Task CloseAsync()
        {
            return ZK?.closeAsync();
        }

        protected override void DisposeCode()
        {
            CloseAsync().Wait();
        }
    }
}