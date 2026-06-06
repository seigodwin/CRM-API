
using System.Text.Json;
using CRMApi.Services.Interfaces;
using Microsoft.Extensions.Caching.Distributed;

namespace CRMApi.Services.Services
{
    public class RedisCacheService : IRedisCacheService
    {
        private readonly IDistributedCache _cache;
        public RedisCacheService(IDistributedCache cache)
        {
            _cache = cache;
        }
        public async Task<T?> GetAsync<T>(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return default;
            }

            var value = await _cache.GetStringAsync(key);
            if(value is null || string.IsNullOrEmpty(value))
            {
                await _cache.RemoveAsync(key);
                return default;
            }
            return JsonSerializer.Deserialize<T>(value);
        }

        public async Task RemoveAsync(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }

             await _cache.RemoveAsync(key);
        }

        public async Task SetAsync<T>(string key, T value, 
        TimeSpan? slidingExpiration = null, TimeSpan? absoluteExpiration = null)
        {
            if (string.IsNullOrEmpty(key) || value is null)
            {
                return;
            }

            var options = new DistributedCacheEntryOptions();
            if (absoluteExpiration.HasValue)
            {
                options.AbsoluteExpirationRelativeToNow = absoluteExpiration;
            }

            if (slidingExpiration.HasValue)
            {
                options.SlidingExpiration = slidingExpiration;
            }
         

            var jsonData = JsonSerializer.Serialize(value);
            await _cache.SetStringAsync(key, jsonData, options);
        }
    }
}