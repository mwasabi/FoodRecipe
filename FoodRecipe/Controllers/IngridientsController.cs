using FoodRecipe.Entity;
using FoodRecipe.Services.IService;
using Microsoft.AspNetCore.Mvc;

namespace FoodRecipe.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class IngridientsController : BaseController<Ingridients>
    {
        public IngridientsController(
            ILogger<IngridientsController> logger,
            IBaseService<Ingridients> service) : base(logger, service)
        {
        }
    }
}