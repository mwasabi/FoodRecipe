using System.ComponentModel.DataAnnotations;

namespace FoodRecipe.DTO.Authentication
{
    public class SendCodeDto
    {
        [Required, EmailAddress, MaxLength(254)]
        public string Email { get; set; } = "";
    }

    public class VerifyCodeDto
    {
        [Required, EmailAddress, MaxLength(254)]
        public string Email { get; set; } = "";

        [Required, StringLength(6, MinimumLength = 6)]
        public string Code { get; set; } = "";
    }

    public class GoogleLoginDto
    {
        [Required]
        public string IdToken { get; set; } = "";
    }

    public class SetNameDto
    {
        [Required, StringLength(50, MinimumLength = 2)]
        public string Name { get; set; } = "";
    }

    public class UserDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = "";
        public string? Name { get; set; }
        public string Role { get; set; } = "";
    }

    public class AuthResultDto
    {
        public string AccessToken { get; set; } = "";

        // true, если после входа нужно показать экран «Как вас зовут?»
        public bool NeedsName { get; set; }

        public UserDto User { get; set; } = null!;
    }
}
