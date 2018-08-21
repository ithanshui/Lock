using org.apache.zookeeper;
using System;
using System.Threading.Tasks;

namespace DotNetLock.ZK
{
    /// <summary>
    /// 功能描述：
    /// 创建人：yjq 2018/8/21 17:17:35
    /// </summary>
    public class ZkWatcher : Watcher
    {
        public readonly Func<WatchedEvent, ZkManager, Task> _changeEvent;
        private readonly ZkManager _zkManager;

        public ZkWatcher(Func<WatchedEvent, ZkManager, Task> changeEvent, ZkManager zkManager)
        {
            _changeEvent = changeEvent;
            _zkManager = zkManager;
        }

        public override Task process(WatchedEvent @event)
        {
            return _changeEvent?.Invoke(@event, _zkManager);
        }
    }
}