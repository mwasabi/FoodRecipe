using FoodRecipe.Entity;
using FoodRecipe.Services.IService;
using Microsoft.AspNetCore.Mvc;

namespace FoodRecipe.Controllers
{  
    [ApiController]
    [Route("[controller]")]
    public class CategoriesController : BaseController<Category>
    { 
        public CategoriesController(
            ILogger<CategoriesController> logger,
            IBaseService<Category> service) : base(logger, service)
        {
        }

    }

}
