using FoodRecipe.DTO.Category;
using FoodRecipe.Entity;
using FoodRecipe.Services.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FoodRecipe.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "User,Admin")]
    public class CategoriesController : BaseController<Category>
    { 
        public CategoriesController(
            ILogger<CategoriesController> logger,
            IBaseService<Category> service) : base(logger, service)
        {
        }
        // 1. СОЗДАНИЕ КАТЕГОРИИ
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CategoryCreateDTO dto, CancellationToken ct = default)
        {
            try
            {
                var category = new Category
                {
                    Id = Guid.NewGuid(),
                    Name = dto.Name,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsDeleted = false
                };

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                _logger.LogInformation("Пользователь {UserId} создает категорию {CategoryName}", userId, dto.Name);

                await _service.AddAsync(category, ct);

                return Created($"/api/Categories/{category.Id}", category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании категории");
                return StatusCode(500, "Внутренняя ошибка сервера.");
            }
        }

        // 2. ОБНОВЛЕНИЕ КАТЕГОРИИ
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] CategoryUpdateDTO dto, CancellationToken ct = default)
        {
            try
            {
                var existingCategory = await _service.GetByIdAsync(id, ct);
                if (existingCategory == null)
                    return NotFound("Категория не найдена.");

                existingCategory.Name = dto.Name;
                existingCategory.UpdatedAt = DateTime.UtcNow;

                await _service.UpdateAsync(existingCategory, ct);

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                _logger.LogInformation("Пользователь {UserId} обновил категорию {CategoryId}", userId, id);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении категории {CategoryId}", id);
                return StatusCode(500, "Внутренняя ошибка сервера.");
            }
        }

    }

}
