using Microsoft.Extensions.Caching.Memory;
using System;
using Volo.Abp.DependencyInjection;

namespace TK.Twitter.Crawl.Tweet.MemoryLock
{
    public class MemoryLock : IDisposable
    {
        private readonly IMemoryCache _memoryCache;

        private static readonly object _lock = new object();

        private string _lockKey;

        public MemoryLock(IMemoryCache memoryCache, string lockKey)
        {
            _memoryCache = memoryCache;
        }

        public MemoryLock TryAcquireLock(string lockKey)
        {
            if (IsTaskInProgress(lockKey))
            {
                return null;
            }

            _lockKey = lockKey;
            SetTaskInProgress(lockKey);
            return this;
        }

        public void Dispose()
        {
            ClearTaskInProgress(_lockKey);
        }

        public bool IsTaskInProgress(string lockKey)
        {
            return _memoryCache.TryGetValue(lockKey, out object __lock) && (__lock != null);
        }

        private void SetTaskInProgress(string lockKey)
        {
            _memoryCache.Set(lockKey, _lock);
        }

        private void ClearTaskInProgress(string lockKey)
        {
            _memoryCache.Remove(lockKey);
        }
    }

    public class MemoryLockProvider : ITransientDependency
    {
        private readonly IMemoryCache _memoryCache;

        public MemoryLockProvider(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public MemoryLock TryAcquireLock(string lockKey)
        {
            var inst = new MemoryLock(_memoryCache, lockKey);
            return inst.TryAcquireLock(lockKey);
        }

        public void ClearLock(string lockKey)
        {
            _memoryCache.Remove(lockKey);
        }
    }
}
