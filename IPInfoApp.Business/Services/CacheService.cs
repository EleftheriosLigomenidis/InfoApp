using Azure;
using IPInfoApp.Business.Contracts;
using IPInfoApp.Business.Utils;
using IPInfoApp.Data.Enums;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace IPInfoApp.Business.Services
{
    public class CacheService(IConnectionMultiplexer redis, ILogger<CacheService> logger) : ICacheService
    {
        private readonly IDatabase _database = redis.GetDatabase();
        private readonly ILogger<CacheService> _logger = logger;

        /// <summary>
        /// Clears the cache for the specific key
        /// </summary>
        /// <param name="key">The key for the cached entity</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task ClearCacheAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                var message = Messages.CacheKeyNull();
                _logger.LogWarning(message);
                throw new ArgumentException(message, nameof(key));
            }

            await _database.KeyDeleteAsync(key);
        }

        /// <summary>
        /// Gets an item using its key from  the redis
        /// </summary>
        /// <param name="key">The key for this entity</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">If the key is null or empty exception will be thrown</exception>
        private async Task<string> GetCacheItemAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                var message = Messages.CacheKeyNull();
                _logger.LogWarning(message);
                throw new ArgumentException(message, nameof(key));
            }

            var cachedResponse = await _database.StringGetAsync(key);

            return cachedResponse.IsNullOrEmpty ? string.Empty : cachedResponse.ToString();

        }

        /// <summary>
        ///  Gets the class T from the cache and converts it to json
        /// </summary>
        /// <typeparam name="T">A class</typeparam>
        /// <param name="key">The cache key</param>
        /// <returns></returns>
        public async Task<T?> TryGetValueFromCacheAsync<T>(string key) where T : class 
        {
            _logger.LogInformation(Messages.FetchEntity(nameof(T), nameof(key), key, Datasource.Cache.GetEnumDescription()));
            T? model = null;
            var cachedValue = await GetCacheItemAsync(key);

            if (!string.IsNullOrEmpty(key))
            {
                model = JsonConvert.DeserializeObject<T>(cachedValue);
            }

            return model;
        }
        /// <summary>
        /// Sets the value in the serialised value in the cache
        /// </summary>
        /// <typeparam name="T">Where T is a class or a list of objects</typeparam>
        /// <param name="key">The cache key</param>
        /// <param name="value">The value to be cached</param>
        /// <param name="timeToLiveInMins">Time to live in minutes</param>
        /// <returns></returns>
        public async Task SetCacheItemAsync<T>(string key, T value, int timeToLiveInMins = 10)
        {
            if (value == null) return;


            var serialisedValue = JsonConvert.SerializeObject(value);

            await _database.StringSetAsync(key, serialisedValue, TimeSpan.FromMinutes(10));
        }

   
    }
}
