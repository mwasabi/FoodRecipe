using FoodRecipe.Entity;

namespace FoodRecipe.Services.IService
{
    public interface IBaseService<TEntity> where TEntity : BaseEntity
    {
        Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken ct = default);

        Task<TEntity?> GetByIdAsync(Guid id, CancellationToken ct = default);

        Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
    }

}
