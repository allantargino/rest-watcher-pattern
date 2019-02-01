using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Watcher.API.Interfaces;

namespace Watcher.API.Providers
{
    public class InMemoryProvider : IProvider<string, string>
    {
        private readonly Dictionary<string, string> storeDictionary;
        private readonly Dictionary<string, Task<string>> actionDictionary;

        public InMemoryProvider()
        {
            storeDictionary = new Dictionary<string, string>();
            actionDictionary = new Dictionary<string, Task<string>>();
        }

        public Task<string> GetAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException(nameof(key));

            return Task.FromResult(storeDictionary[key]);
        }

        public Task SetAsync(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException(nameof(key));
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException(nameof(value));

            storeDictionary[key] = value;

            return Task.CompletedTask;
        }

        private void Notify(string key)
        {
            if (actionDictionary.ContainsKey(key))
            {
                var action = actionDictionary[key];
                action.Start();
            }
        }

        public async Task<string> WatchAsync(string key, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException(nameof(key));

            Task<string> keyWatcher = new Task<string>(() =>
            {
                return storeDictionary[key];
            }, cancellationToken);

            actionDictionary[key] = keyWatcher;

            string newValue = await keyWatcher;

            return newValue;
        }

        public async Task SetAndNotifyAsync(string key, string value)
        {
            await SetAsync(key, value);
            Notify(key);
        }
    }
}
