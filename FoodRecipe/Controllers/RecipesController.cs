
using Microsoft.AspNetCore.Mvc;
using FoodRecipe.DTO;
using FoodRecipe.Entity;
using FoodRecipe.Services.IService;

namespace FoodRecipe.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RecipesController : BaseController<Recipe>
    {
        private readonly IRecipeService _recipeService;

        // Передаем логгер и сервис рецептов в ваш базовый контроллер
        public RecipesController(
            ILogger<RecipesController> logger,
            IRecipeService recipeService) : base(logger, recipeService)
        {
            _recipeService = recipeService;
        }
    }
}

