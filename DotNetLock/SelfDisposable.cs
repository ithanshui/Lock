using System;

namespace DotNetLock
{
    /// <summary>
    /// 功能描述：
    /// 创建人：yjq 2018/8/21 17:17:09
    /// </summary>
    public class SelfDisposable : IDisposable
    {
        private bool _isDisposed;

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void DisposeCode()
        {
        }

        private void Dispose(bool disposing)
        {
            if (!_isDisposed && disposing)
            {
                DisposeCode();
            }
            _isDisposed = true;
        }

        ~SelfDisposable()
        {
            Dispose(false);
        }
    }
}