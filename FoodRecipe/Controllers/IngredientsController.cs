using FoodRecipe.DTO.Ingredients;
using FoodRecipe.DTO.Ingredients.FoodRecipe.DTO.Ingredients;
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
    public class IngredientsController : BaseController<Ingredients>
    {
        private readonly IBaseService<Recipe> _recipeService;

        public IngredientsController(
            ILogger<IngredientsController> logger,
            IBaseService<Ingredients> service,
            IBaseService<Recipe> recipeService) : base(logger, service)
        {
            _recipeService = recipeService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] IngredientsCreateDTO dto, CancellationToken ct = default)
        {
            try
            {
                if (dto.Ingredients == null || dto.Ingredients.Count == 0)
                    return BadRequest("Список ингредиентов пуст.");

                var recipe = await _recipeService.GetByIdAsync(dto.RecipeId, ct);
                if (recipe == null)
                    return NotFound("Рецепт не найден.");

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (recipe.UserId != userId && !User.IsInRole("Admin"))
                    return Forbid();

                var created = new List<object>();

                foreach (var item in dto.Ingredients)
                {
                    var ingredient = new Ingredients
                    {
                        Id = Guid.NewGuid(),
                        Name = item.Name,
                        Quantity = item.Quantity,
                        Unit = item.Unit,
                        RecipeId = dto.RecipeId,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        IsDeleted = false
                    };

                    await _service.AddAsync(ingredient, ct);
                    created.Add(new { ingredient.Id, ingredient.Name, ingredient.Quantity, ingredient.Unit });
                }

                _logger.LogInformation("Пользователь {UserId} добавил {Count} ингредиентов в рецепт {RecipeId}",
                    userId, dto.Ingredients.Count, dto.RecipeId);

                return Created($"/api/Recipes/{dto.RecipeId}", created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании ингредиентов");
                return StatusCode(500, "Внутренняя ошибка сервера.");
            }
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] IngredientsUpdateDTO dto, CancellationToken ct = default)
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
                existing.Quantity = dto.Quantity;
                existing.Unit = dto.Unit;
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