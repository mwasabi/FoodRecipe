using FoodRecipe.DTO.Authentication;
using FoodRecipe.Services.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace FoodRecipe.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [EnableRateLimiting("auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _auth;

        public AuthController(IAuthService auth)
        {
            _auth = auth;
        }

        // 1. Отправить код на email (вход и регистрация одним и тем же способом)
        [HttpPost("email/send-code")]
        public async Task<IActionResult> SendCode([FromBody] SendCodeDto dto, CancellationToken ct)
        {
            try
            {
                await _auth.SendCodeAsync(dto.Email, ct);
                return Ok(new { message = "Код отправлен" });
            }
            catch (AuthException ex)
            {
                return Fail(ex);
            }
        }

        // 2. Проверить код и получить токен
        [HttpPost("email/verify")]
        public async Task<IActionResult> Verify([FromBody] VerifyCodeDto dto, CancellationToken ct)
        {
            try
            {
                return Ok(await _auth.VerifyCodeAsync(dto.Email, dto.Code, ct));
            }
            catch (AuthException ex)
            {
                return Fail(ex);
            }
        }

        // 3. Вход через Google (фронтенд присылает idToken)
        [HttpPost("google")]
        public async Task<IActionResult> Google([FromBody] GoogleLoginDto dto, CancellationToken ct)
        {
            try
            {
                return Ok(await _auth.GoogleAsync(dto.IdToken, ct));
            }
            catch (AuthException ex)
            {
                return Fail(ex);
            }
        }

        // 4. Данные текущего пользователя
        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> Me(CancellationToken ct)
        {
            if (!TryGetUserId(out var userId)) return Unauthorized();

            try
            {
                return Ok(await _auth.GetMeAsync(userId, ct));
            }
            catch (AuthException ex)
            {
                return Fail(ex);
            }
        }

        // 5. Сохранить имя после первого входа
        [Authorize]
        [HttpPut("name")]
        public async Task<IActionResult> SetName([FromBody] SetNameDto dto, CancellationToken ct)
        {
            if (!TryGetUserId(out var userId)) return Unauthorized();

            try
            {
                return Ok(await _auth.SetNameAsync(userId, dto.Name, ct));
            }
            catch (AuthException ex)
            {
                return Fail(ex);
            }
        }

        private bool TryGetUserId(out Guid userId) =>
            Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out userId);

        private ObjectResult Fail(AuthException ex) =>
            StatusCode(ex.StatusCode, new { message = ex.Message });
    }
}