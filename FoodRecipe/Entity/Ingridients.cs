namespace FoodRecipe.Entity
{
    public class Ingridients : BaseEntity
    {
        public string Name { get; set; }
        public int Quantity { get; set; }
        public Guid RecipeId { get; set; }
        public Recipe? Recipe { get; set; }
    }
}
