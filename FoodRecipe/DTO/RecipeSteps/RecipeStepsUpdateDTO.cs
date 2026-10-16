using FoodRecipe.DTO.RecipeSteps;

namespace FoodRecipe.DTO.RecipeSteps
{
    public class RecipeStepsUpdateDTO
    {
        public int StepNumber { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}