using FoodRecipe.Data;
using FoodRecipe.DTO;
using FoodRecipe.Entity;
using FoodRecipe.Services.IService;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Repositories;

namespace FoodRecipe.Services.Service
{
    public class RecipeService : BaseService<Recipe>, IRecipeService
    {
        private readonly AppDbContext _context;
        private readonly IPostgreSQLRepository<Recipe> _recipeRepository;

        public RecipeService(
            IPostgreSQLRepository<Recipe> repository,
            IConfiguration config,
            AppDbContext context) : base(repository, config, context)
        {
            _context = context;
            _recipeRepository = repository;
        }
    }
}

