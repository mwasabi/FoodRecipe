public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string TabGroup { get; set; } = string.Empty;
    public string SearchTag { get; set; } = string.Empty; // <-- ДОБАВЬ ЭТУ СТРОКУ
}
