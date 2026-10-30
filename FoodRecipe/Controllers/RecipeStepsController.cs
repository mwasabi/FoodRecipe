using FoodRecipe.DTO.RecipeSteps;
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
    public class RecipeStepsController : BaseController<RecipeSteps>
    {
        private readonly IBaseService<Recipe> _recipeService;

        public RecipeStepsController(
            ILogger<RecipeStepsController> logger,
            IBaseService<RecipeSteps> service,
            IBaseService<Recipe> recipeService) : base(logger, service)
        {
            _recipeService = recipeService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] RecipeStepsCreateDTO dto, CancellationToken ct = default)
        {
            try
            {
                if (dto.Steps == null || dto.Steps.Count == 0)
                    return BadRequest("Список шагов пуст.");

                var recipe = await _recipeService.GetByIdAsync(dto.RecipeId, ct);
                if (recipe == null)
                    return NotFound("Рецепт не найден.");

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (recipe.UserId != userId && !User.IsInRole("Admin"))
                    return Forbid();

                var created = new List<object>();

                foreach (var item in dto.Steps)
                {
                    var step = new RecipeSteps
                    {
                        Id = Guid.NewGuid(),
                        StepNumber = item.StepNumber,
                        Description = item.Description,
                        RecipeId = dto.RecipeId,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        IsDeleted = false
                    };

                    await _service.AddAsync(step, ct);
                    created.Add(new { step.Id, step.StepNumber, step.Description });
                }

                return Created($"/api/Recipes/{dto.RecipeId}", created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании шагов");
                return StatusCode(500, "Внутренняя ошибка сервера.");
            }
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] RecipeStepsUpdateDTO dto, CancellationToken ct = default)
        {
            try
            {
                var existing = await _service.GetByIdAsync(id, ct);
                if (existing == null)
                    return NotFound("Шаг не найден.");

                var recipe = await _recipeService.GetByIdAsync(existing.RecipeId, ct);
                if (recipe == null)
                    return NotFound("Рецепт не найден.");

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (recipe.UserId != userId && !User.IsInRole("Admin"))
                    return Forbid();

                existing.StepNumber = dto.StepNumber;
                existing.Description = dto.Description;
                existing.UpdatedAt = DateTime.UtcNow;

                await _service.UpdateAsync(existing, ct);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении шага {Id}", id);
                return StatusCode(500, "Внутренняя ошибка сервера.");
            }
        }
    }
}