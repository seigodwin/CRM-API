

namespace CRMApi.Repository
{
    public interface IBaseRepository<T>
    {
        Task AddAsync(T entity);
        Task<IEnumerable<T>> GetAllAsync(int page , int pageSize);
        Task<T?> GetById(int id);
        Task DeleteAsync(T entity);
        Task UpdateAsync(T entity);
    }
}