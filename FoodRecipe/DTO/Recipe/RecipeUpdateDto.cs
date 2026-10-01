namespace FoodRecipe.DTO.Recipe;
public class RecipeUpdateDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int CookingTimeMinutes { get; set; }
        public Guid CategoryId { get; set; }
    }