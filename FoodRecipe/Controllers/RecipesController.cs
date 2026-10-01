using FoodRecipe.DTO; // Подключаем твои DTO
using FoodRecipe.DTO.Recipe;
using FoodRecipe.Entity;
using FoodRecipe.Services.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FoodRecipe.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RecipesController : BaseController<Recipe>
{
    public RecipesController(
        ILogger<RecipesController> logger,
        IBaseService<Recipe> service)
        : base(logger, service)
    {
    }

    // 1. СОЗДАНИЕ РЕЦЕПТА ЧЕРЕЗ DTO (Доступно User и Admin)
    [HttpPost]
    [Authorize(Roles = "User,Admin")]
    public async Task<IActionResult> Create([FromBody] RecipeCreateDto dto, CancellationToken ct = default)
    {
        try
        {
            // Извлекаем ID авторизованного пользователя из JWT токена
            // Ищем ID по чистому имени ключа "id"
            var currentUserId = User.FindFirst("id")?.Value;

            if (string.IsNullOrEmpty(currentUserId) || !Guid.TryParse(currentUserId, out Guid authorId))
            {
                return Unauthorized("Пользователь не авторизован.");
            }

            // Маппим DTO в реальную сущность Recipe (скрывая системные поля)
            var recipe = new Recipe
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Description = dto.Description,
                CookingTimeMinutes = dto.CookingTimeMinutes,
                CategoryId = dto.CategoryId, // Привязываем к выбранной категории
                AuthorId = authorId,          // Автоматически ставим автора
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            _logger.LogInformation("Пользователь {UserId} создает рецепт в категории {CategoryId}", authorId, dto.CategoryId);

            await _service.AddAsync(recipe, ct);

            return CreatedAtAction(nameof(GetByIdAsync), new { id = recipe.Id }, recipe);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при создании рецепта через DTO");
            return StatusCode(500, "Внутренняя ошибка сервера.");
        }
    }

    // 2. ОБНОВЛЕНИЕ РЕЦЕПТА ЧЕРЕЗ DTO (Только Автор или Админ)
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "User,Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] RecipeUpdateDto dto, CancellationToken ct = default)
    {
        try
        {
            // Получаем старый рецепт из БД для проверки авторства
            var existingRecipe = await _service.GetByIdAsync(id, ct);
            if (existingRecipe == null) return NotFound("Рецепт не найден.");

            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            bool isAdmin = User.IsInRole("Admin");

            // ПРОВЕРКА: Если не админ и не автор этого рецепта -> отказ в доступе
            if (!isAdmin && existingRecipe.AuthorId.ToString() != currentUserId)
            {
                _logger.LogWarning("Отказ в редактировании. Пользователь {UserId} не является автором рецепта {RecipeId}", currentUserId, id);
                return Forbid();
            }

            // Обновляем только разрешенные поля из DTO
            existingRecipe.Name = dto.Name;
            existingRecipe.Description = dto.Description;
            existingRecipe.CookingTimeMinutes = dto.CookingTimeMinutes;
            existingRecipe.CategoryId = dto.CategoryId;
            existingRecipe.UpdatedAt = DateTime.UtcNow;

            _logger.LogInformation("Рецепт {RecipeId} успешно обновлен пользователем {UserId}", id, currentUserId);

            await _service.UpdateAsync(existingRecipe, ct);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении рецепта {RecipeId}", id);
            return StatusCode(500, "Внутренняя ошибка сервера.");
        }
    }
}
