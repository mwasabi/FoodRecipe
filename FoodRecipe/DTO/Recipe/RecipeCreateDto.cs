namespace FoodRecipe.DTO.Recipe;

public class RecipeCreateDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int CookingTimeMinutes { get; set; }
    public Guid CategoryId { get; set; } // Передаем только ID категории!
}

