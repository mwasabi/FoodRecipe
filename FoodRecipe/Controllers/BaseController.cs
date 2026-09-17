using FoodRecipe.Entity;
using FoodRecipe.Services.IService;
using Microsoft.AspNetCore.Mvc;

namespace FoodRecipe.Controllers
{
    public class BaseController<TEntity> : ControllerBase where TEntity : BaseEntity
    {
        protected readonly IBaseService<TEntity> _service;
        protected readonly ILogger<BaseController<TEntity>> _logger;
        public BaseController(
            ILogger<BaseController<TEntity>> logger,
            IBaseService<TEntity> service)
        {
            _logger = logger;
            _service = service;
        }
        [HttpGet("AllItems")]
        public async virtual Task<IEnumerable<TEntity>> GetAsync()
        {
            return await _service.GetAllAsync();
        }
        [HttpGet("GetItemById")]
        public async virtual Task<TEntity> GetByIdAsync(Guid id)
        {
            return await _service.GetByIdAsync(id);
        }
        [HttpPost("Create")]
        public virtual async Task<TEntity> PostAsync([FromBody] TEntity item)
        {
            return await _service.CreateAsync(item);
        }
        [HttpPut("Update")]
        public virtual async Task<bool> PutAsync([FromQuery] Guid id, [FromBody] TEntity item)
        {
            return await _service.UpdateAsync(id, item);
        }
        [HttpDelete("Delete")]
        public virtual async Task<bool> DeleteAsync([FromQuery] Guid id)
        {
            return await _service.DeleteAsync(id);
        }
    }
}  
