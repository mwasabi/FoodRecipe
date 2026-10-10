namespace FoodRecipe.DTO.Ingridients
{
    public class IngridientsCreateDTO
    {
        public string Name { get; set; } = string.Empty;
        public Guid RecipeId { get; set; }
    }
}
