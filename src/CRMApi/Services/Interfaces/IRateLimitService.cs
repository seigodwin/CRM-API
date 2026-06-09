
namespace CRMApi.Services.Interfaces
{
    public interface IRateLimitService
    {
        Task<bool> IsRateLimited (string key, int limit, TimeSpan window);
    }
}