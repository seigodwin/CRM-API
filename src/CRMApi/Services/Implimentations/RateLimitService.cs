
using CRMApi.Services.Interfaces;
using StackExchange.Redis;

namespace CRMApi.Services.Services
{
    public class RateLimitService : IRateLimitService
    {
        private readonly IDatabase _redisCache;
        public RateLimitService(IConnectionMultiplexer redis)
        {
            _redisCache = redis.GetDatabase();
        }
        public async Task<bool> IsRateLimited(string key, int limit, TimeSpan window)
        { 
            long attempt = 0;

            if (!string.IsNullOrEmpty(key))
            {
                attempt = await _redisCache.StringIncrementAsync(key);
            }

            if(attempt == 1)
            {
                await _redisCache.KeyExpireAsync(key, window);
            }
            return attempt > limit ? true : false;
        }
    }
}