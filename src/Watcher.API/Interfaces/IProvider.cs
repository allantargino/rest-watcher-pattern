using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Watcher.API.Interfaces
{
    public interface IProvider<TKey, TValue>
    {
        Task SetAsync(TKey key, TValue value);
        Task SetAndNotifyAsync(TKey key, TValue value);
        Task<TValue> GetAsync(TKey key);
        Task<TValue> WatchAsync(TKey key, CancellationToken cancellationToken);
    }
}
