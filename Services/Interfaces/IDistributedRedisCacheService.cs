
namespace CRMApi.Services.Interfaces
{
    public interface IDistributedRedisCacheService
    {
        Task<T?> GetAsync<T>(string key);
        Task SetAsync<T>(string key, T value, 
        TimeSpan? slidingExpiration = null, TimeSpan? absoluteExpiration = null);
        Task RemoveAsync(string key);
    }
}