using FoodRecipe.DTO.RecipeSteps;

namespace FoodRecipe.DTO.RecipeSteps
{
    public class RecipeStepItemDTO
    {
        public int StepNumber { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    public class RecipeStepsCreateDTO
    {
        public Guid RecipeId { get; set; }
        public List<RecipeStepItemDTO> Steps { get; set; } = new();
    }
}