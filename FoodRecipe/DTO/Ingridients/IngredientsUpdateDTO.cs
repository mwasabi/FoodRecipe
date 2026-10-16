namespace FoodRecipe.DTO.Ingredients
{
    public class IngredientsUpdateDTO
    {
        public string Name { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string Unit { get; set; } = string.Empty;
    }
}
