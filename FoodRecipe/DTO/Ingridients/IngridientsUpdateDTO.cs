namespace FoodRecipe.DTO.Ingridients
{
    public class IngridientsUpdateDTO
    {
        public string Name { get; set; } = string.Empty;
        public Guid RecipeId { get; set; }
    }
}
