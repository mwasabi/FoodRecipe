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
[Authorize(Roles = "User,Admin")]   
public class RecipesController : BaseController<Recipe>
{
    public RecipesController(
        ILogger<RecipesController> logger,
        IBaseService<Recipe> service)
        : base(logger, service)
    {
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] RecipeCreateDTO dto, CancellationToken ct = default)
    {
        try
        {
            var recipe = new Recipe
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Description = dto.Description,
                UserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation("Пользователь {UserId} создает рецепт {RecipeName}", userId, dto.Name);

            await _service.AddAsync(recipe, ct);
            return Created($"/api/Recipes/{recipe.Id}", recipe);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при создании рецепта");
            return StatusCode(500, "Внутренняя ошибка сервера.");
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] RecipeUpdateDTO dto, CancellationToken ct = default)
    {
        try
        {
            var existing = await _service.GetByIdAsync(id, ct);
            if (existing == null)
                return NotFound("Рецепт не найден.");

            existing.Name = dto.Name;
            existing.Description = dto.Description;
            existing.UpdatedAt = DateTime.UtcNow;

            await _service.UpdateAsync(existing, ct);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении рецепта {RecipeId}", id);
            return StatusCode(500, "Внутренняя ошибка сервера.");
        }
    }
}
