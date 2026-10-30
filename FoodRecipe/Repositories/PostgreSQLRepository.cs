using FoodRecipe.Data;
using FoodRecipe.Entity;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using WebApplication1.Repositories;

namespace FoodRecipe.Repositories
{
    public class PostgreSQLRepository<T> : IPostgreSQLRepository<T> where T : BaseEntity
    {
        readonly AppDbContext _context;
        private readonly DbSet<T> _dbSet;

        public PostgreSQLRepository(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = _context.Set<T>();
        }

        // EF Core сам не подгружает связанные таблицы, их нужно указать через Include.
        // Для рецепта подгружаем ингредиенты и категорию, для остальных сущностей ничего не меняется.
        private static IQueryable<T> WithIncludes(IQueryable<T> query)
        {
            if (typeof(T) == typeof(Recipe))
            {
                var recipes = ((IQueryable<Recipe>)(object)query)
                    .Include(r => r.Ingredients)
                    .Include(r => r.Steps)
                    .Include(r => r.Category);

                return (IQueryable<T>)(object)recipes;
            }

            return query;
        }

        public async Task<IEnumerable<T>> GetAllAsync(bool isAdmin, CancellationToken ct, Expression<Func<T, bool>>? filter = null)
        {
            IQueryable<T> query = WithIncludes(_dbSet.AsNoTracking());

            // Обычные пользователи не видят удалённые (soft delete) записи, админ видит всё
            if (!isAdmin)
            {
                query = query.Where(e => !e.IsDeleted);
            }

            if (filter != null)
            {
                query = query.Where(filter);
            }

            return await query.ToListAsync(ct);
        }

        public async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await WithIncludes(_dbSet).FirstOrDefaultAsync(e => e.Id == id, ct);
        }

        public async Task<T> AddAsync(T item, CancellationToken ct = default)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            await _dbSet.AddAsync(item, ct);
            await _context.SaveChangesAsync(ct);
            return item;
        }

        public async Task<bool> UpdateAsync(T entity, CancellationToken ct = default)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            // Помечаем изменённой только саму сущность, а не весь граф связанных данных
            // (иначе вместе с рецептом перезаписывались бы все его ингредиенты)
            _context.Entry(entity).State = EntityState.Modified;
            return await _context.SaveChangesAsync(ct) > 0;
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var entity = await GetByIdAsync(id, ct);
            if (entity == null)
                return false;

            _dbSet.Remove(entity);
            return await _context.SaveChangesAsync(ct) > 0;
        }

        public async Task<(IEnumerable<T> Items, int TotalCount)> GetPageAsync(int page, int size, CancellationToken ct = default)
        {
            if (page <= 0) page = 1;
            if (size <= 0) size = 10;

            var total = await _dbSet.CountAsync(ct);
            var items = await WithIncludes(_dbSet.AsNoTracking())
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync(ct);

            return (items, total);
        }

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            return await WithIncludes(_dbSet.AsNoTracking())
                .Where(predicate)
                .ToListAsync(ct);
        }
    }
}