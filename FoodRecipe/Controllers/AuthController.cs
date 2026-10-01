using FoodRecipe.Data;
using FoodRecipe.DTO.Authentication;
using FoodRecipe.Entity;
using FoodRecipe.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FoodRecipe.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
            { 
                return BadRequest("Пользователь с таким Email уже существует.");
            }

            var passwordHash = Convert.ToBase64String(Encoding.UTF8.GetBytes(dto.Password));

            var user = new User
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Email = dto.Email,
                PasswordHash = passwordHash,
                Role = UserRole.User
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok("Регистрация прошла успешно.");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null) return BadRequest("Неверный Email или пароль.");
            
            var incomigHash = Convert.ToBase64String(Encoding.UTF8.GetBytes(dto.Password));
            if (user.PasswordHash != incomigHash) return BadRequest("Неверный Email или пароль.");

            var claims = new[]
{
    new Claim("id", user.Id.ToString()), // <-- Заменили ClaimTypes.NameIdentifier на простой "id"
    new Claim(ClaimTypes.Name, user.Name),
    new Claim(ClaimTypes.Role, user.Role.ToString())
};

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("SuperSecretKeyWithLongLengthForSecurity123!"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = creds,

                // !!! ДОБАВЛЯЕМ ЭТИ ДВЕ СТРОЧКИ СЮДА !!!
                Issuer = "FoodRecipeServer",
                Audience = "FoodRecipeClient"
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return Ok(new { Token = tokenString });

        }
    }
}
