using FoodRecipe.Entity;
using FoodRecipe.Services.IService;
using Microsoft.AspNetCore.Authorization;
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
        public async virtual Task<ActionResult<IEnumerable<TEntity>>> GetAll(CancellationToken ct = default)
        {
            try 
            {
                _logger.LogInformation("HTTP GET All для {EntityType}", typeof(TEntity).Name);

                // Проверяем роль только если пользователь вообще прошел аутентификацию
                bool isAdmin = User.Identity != null && User.Identity.IsAuthenticated && User.IsInRole("Admin");


                var items = await _service.GetAllAsync(isAdmin, ct);
                
                return Ok(items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в GetAll: {Message}", ex.Message);
                return StatusCode(500, "Внутренняя ошибка сервера.");
            }   

        }
        [HttpGet("GetItemById")]
        public async virtual Task<TEntity> GetByIdAsync(Guid id)
        {
            return await _service.GetByIdAsync(id);
        }

        [HttpDelete("{id:guid}/permanent")]
        [AllowAnonymous] // Временно для тестов без авторизации
        public virtual async Task<IActionResult> PermanentDelete(Guid id, CancellationToken ct = default)
        {
            try
            {
                _logger.LogWarning("HTTP DELETE PERMANENT (HARD DELETE) — Id: {Id} | Пользователь: {User}", id, User.Identity?.Name ?? "аноним");

                // Вызываем жесткое удаление из сервиса
                var success = await _service.DeleteAsync(id, ct);

                if (!success)
                {
                    _logger.LogWarning("Hard Delete: сущность не найдена — Id: {Id}", id);
                    return NotFound($"Сущность с Id = {id} не найдена.");
                }

                _logger.LogCritical("ВНИМАНИЕ! ВЫПОЛНЕНО ПОЛНОЕ УДАЛЕНИЕ ИЗ БД — Id: {Id} | Тип: {EntityType}", id, typeof(TEntity).Name);
                return NoContent(); // 204
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Критическая ошибка при Hard Delete — Id: {Id}", id);
                return StatusCode(500, "Внутренняя ошибка сервера.");
            }
        }

        [HttpDelete("{id:guid}")]
        [AllowAnonymous]
        public virtual async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
        {
            try
            {
                _logger.LogInformation("HTTP DELETE (soft) - Id: {Id} | Пользователь: {User}", id, User.Identity?.Name ?? "аноним");
                
                var success = await _service.SoftDeleteAsync(id, ct);

                if (!success)
                {
                    _logger.LogWarning("SoftDelete: Сущность не найдена или уже удалена - Id: {Id}", id);
                    return NotFound();
                }

                _logger.LogInformation("SoftDelete успешно выполнен - Id: {Id}", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Критическая ошибка при Soft Delete - Id: {Id}", id);
                return StatusCode(500, "Внутренняя ошибка сервера.");
            }
        }
    }
}  
