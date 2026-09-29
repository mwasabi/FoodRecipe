using System.Linq.Expressions;

namespace WebApplication1.Repositories
{
    public interface IPostgreSQLRepository<T>
    {
        Task<IEnumerable<T>> GetAllAsync(bool isAdmin, CancellationToken ct, Expression<Func<T, bool>>? filter = null);
        Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<T> CreateAsync(T item, CancellationToken ct = default);
        Task<bool> UpdateAsync(T entity, CancellationToken ct = default);
        Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
    }

}
