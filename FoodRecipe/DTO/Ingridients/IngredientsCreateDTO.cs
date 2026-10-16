namespace FoodRecipe.DTO.Ingredients
{
    namespace FoodRecipe.DTO.Ingredients
    {
        public class IngredientItemDTO
        {
            public string Name { get; set; } = string.Empty;
            public decimal Quantity { get; set; }
            public string Unit { get; set; } = string.Empty;
        }

        public class IngredientsCreateDTO
        {
            public Guid RecipeId { get; set; }
            public List<IngredientItemDTO> Ingredients { get; set; } = new();
        }
    }
}