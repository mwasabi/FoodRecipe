using FoodRecipe.Data;
using FoodRecipe.Entity;
using FoodRecipe.Services.IService;
using WebApplication1.Repositories;

namespace FoodRecipe.Services.Service
{
    public class BaseService<T> : IBaseService<T> where T : BaseEntity
    {
        private readonly IPostgreSQLRepository<T> _repository;
        private readonly AppDbContext _context;

        public BaseService(
            IPostgreSQLRepository<T> repository,
            AppDbContext context
            )
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<T>> GetAllAsync(bool isAdmin, CancellationToken ct = default)
        {
            return await _repository.GetAllAsync(isAdmin, ct);
        }

        public async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _repository.GetByIdAsync(id, ct);
        }

       
        public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var existing = await _repository.GetByIdAsync(id, ct);
            if (existing == null)
                return false;

            return await _repository.DeleteAsync(id, ct);
        }
        public virtual async Task<bool> SoftDeleteAsync(Guid id, CancellationToken ct = default)
        {
            var entity = await _repository.GetByIdAsync(id, ct);

            if (entity == null || entity.IsDeleted)
            {
                return false;
            }

            entity.IsDeleted = true;

            await _repository.UpdateAsync(entity, ct);
            return true;
        }
    }


}
