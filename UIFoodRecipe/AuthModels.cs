namespace UIFoodRecipe
{
    public class SendCodeModel
    {
        public string Email { get; set; } = string.Empty;
    }

    public class VerifyCodeModel
    {
        public string Email { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }
}
