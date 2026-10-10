using FoodRecipe.DTO.Ingridients;
using FoodRecipe.Entity;
using FoodRecipe.Services.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FoodRecipe.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize(Roles = "User,Admin")]
    public class IngridientsController : BaseController<Ingridients>
    {
        private readonly IBaseService<Recipe> _recipeService;

        public IngridientsController(
            ILogger<IngridientsController> logger,
            IBaseService<Ingridients> service,
            IBaseService<Recipe> recipeService) : base(logger, service)
        {
            _recipeService = recipeService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] IngridientsCreateDTO dto, CancellationToken ct = default)
        {
            try
            {
                var recipe = await _recipeService.GetByIdAsync(dto.RecipeId, ct);
                if (recipe == null)
                    return NotFound("Рецепт не найден.");

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (recipe.UserId != userId && !User.IsInRole("Admin"))
                    return Forbid();

                var ingridient = new Ingridients
                {
                    Id = Guid.NewGuid(),
                    Name = dto.Name,
                    RecipeId = dto.RecipeId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsDeleted = false
                };

                await _service.AddAsync(ingridient, ct);
                _logger.LogInformation("Пользователь {UserId} добавил ингредиент {Name} в рецепт {RecipeId}",
                    userId, dto.Name, dto.RecipeId);

                return Created($"/api/Ingridients/{ingridient.Id}", ingridient);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании ингредиента");
                return StatusCode(500, "Внутренняя ошибка сервера.");
            }
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] IngridientsUpdateDTO dto, CancellationToken ct = default)
        {
            try
            {
                var existing = await _service.GetByIdAsync(id, ct);
                if (existing == null)
                    return NotFound("Ингредиент не найден.");

                var recipe = await _recipeService.GetByIdAsync(existing.RecipeId, ct);
                if (recipe == null)
                    return NotFound("Рецепт не найден.");

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (recipe.UserId != userId && !User.IsInRole("Admin"))
                    return Forbid();

                existing.Name = dto.Name;
                existing.UpdatedAt = DateTime.UtcNow;

                await _service.UpdateAsync(existing, ct);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении ингредиента {Id}", id);
                return StatusCode(500, "Внутренняя ошибка сервера.");
            }
        }
    }
}