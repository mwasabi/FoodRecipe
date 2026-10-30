using FoodRecipe.DTO.Authentication;
using FoodRecipe.Entity;

namespace FoodRecipe.Services.IService
{
    public interface IBaseService<TEntity> where TEntity : BaseEntity
    {
        Task<IEnumerable<TEntity>> GetAllAsync(bool isAdmin, CancellationToken ct = default);

        Task<TEntity?> GetByIdAsync(Guid id, CancellationToken ct = default);

        Task<TEntity> AddAsync(TEntity entity, CancellationToken ct = default);

        Task<bool> UpdateAsync(TEntity entity, CancellationToken ct = default);

        Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
        
        Task<bool> SoftDeleteAsync(Guid id, CancellationToken ct = default);
    }
    public interface IAuthService
    {
        // Отправить код подтверждения на email
        Task SendCodeAsync(string email, CancellationToken ct = default);

        // Проверить код и выдать токен (если пользователя нет, он создаётся)
        Task<AuthResultDto> VerifyCodeAsync(string email, string code, CancellationToken ct = default);

        // Войти через Google по idToken
        Task<AuthResultDto> GoogleAsync(string idToken, CancellationToken ct = default);

        // Данные текущего пользователя
        Task<UserDto> GetMeAsync(Guid userId, CancellationToken ct = default);

        // Сохранить имя после первого входа
        Task<UserDto> SetNameAsync(Guid userId, string name, CancellationToken ct = default);
    }
    public class AuthException : Exception
    {
        public int StatusCode { get; }

        public AuthException(string message, int statusCode = 400) : base(message)
        {
            StatusCode = statusCode;
        }
    }
    public interface IEmailSender
    {
        Task SendAsync(string to, string subject, string body, CancellationToken ct = default);
    }
}
