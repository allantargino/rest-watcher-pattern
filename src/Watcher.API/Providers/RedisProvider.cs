using StackExchange.Redis;
using System.Threading;
using System.Threading.Tasks;
using Watcher.API.Interfaces;

namespace Watcher.API.Providers
{
    public class RedisProvider : IProvider<string, string>
    {
        private readonly ConnectionMultiplexer redis;
        private readonly IDatabase database;
        private readonly ISubscriber sub;

        public RedisProvider(string endpoint = "localhost")
        {
            this.redis = ConnectionMultiplexer.Connect(endpoint);
            this.database = redis.GetDatabase();
            this.sub = redis.GetSubscriber();
        }

        public async Task<string> GetAsync(string key)
        {
            var value = await database.StringGetAsync(key);

            return value.ToString();
        }

        public async Task SetAndNotifyAsync(string key, string value)
        {
            await SetAsync(key, value);

            await sub.PublishAsync(key, value);
        }

        public async Task SetAsync(string key, string value)
        {
            await database.StringSetAsync(key, value);
        }

        public async Task<string> WatchAsync(string key, CancellationToken cancellationToken)
        {
            string lastValue = string.Empty;

            SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);
            await semaphoreSlim.WaitAsync();

            await sub.SubscribeAsync(key, (channel, message) =>
            {
                lastValue = message;
                semaphoreSlim.Release();
            });

            await semaphoreSlim.WaitAsync(cancellationToken);

            return lastValue;
        }
    }
}
