namespace FoodRecipe.Entity
{
    public class RecipeSteps : BaseEntity
    {
        public int StepNumber { get; set; }
        public string Description { get; set; } = string.Empty;
        public Guid RecipeId { get; set; }
        public Recipe Recipe { get; set; } = null!;
    }
}