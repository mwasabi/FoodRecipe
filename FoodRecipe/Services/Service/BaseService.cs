using FoodRecipe.Data;
using FoodRecipe.Entity;
using FoodRecipe.Services.IService;
using WebApplication1.Repositories;

namespace FoodRecipe.Services.Service
{
    public class BaseService<T> : IBaseService<T> where T : BaseEntity
    {
        private readonly IConfiguration _config;
        private readonly AppDbContext _context;

        IPostgreSQLRepository<T> _repository;

        public BaseService(
            IPostgreSQLRepository<T> repository,
            IConfiguration config,
            AppDbContext context
            )
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default)
        {
            return await _repository.GetAllAsync(ct);
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
    }


}
