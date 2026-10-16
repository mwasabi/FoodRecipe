using FoodRecipe.Data;
using FoodRecipe.DTO.Authentication;
using FoodRecipe.Entity;
using FoodRecipe.Enums;
using FoodRecipe.Services.IService;
using Google.Apis.Auth;
using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MimeKit;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Mail;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using WebApplication1.Repositories;

namespace FoodRecipe.Services.Service
{
    public class BaseService<T> : IBaseService<T> where T : BaseEntity
    {
        private readonly IPostgreSQLRepository<T> _repository;
        private readonly AppDbContext _context;

        public BaseService(
            IPostgreSQLRepository<T> repository,
            AppDbContext context
            )
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<T>> GetAllAsync(bool isAdmin, CancellationToken ct = default)
        {
            return await _repository.GetAllAsync(isAdmin, ct);
        }

        public async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _repository.GetByIdAsync(id, ct);
        }

       
        public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var existing = await _repository.GetByIdAsync(id, ct);
            if (existing == null)
                return false;

            return await _repository.DeleteAsync(id, ct);
        }
        public virtual async Task<bool> SoftDeleteAsync(Guid id, CancellationToken ct = default)
        {
            var entity = await _repository.GetByIdAsync(id, ct);

            if (entity == null || entity.IsDeleted)
            {
                return false;
            }

            entity.IsDeleted = true;

            await _repository.UpdateAsync(entity, ct);
            return true;
        }
        public async Task<T> AddAsync(T entity, CancellationToken ct = default)
        {
            // Вызываем метод добавления из твоего репозитория
            await _repository.AddAsync(entity, ct);
            return entity;
        }

        public async Task<bool> UpdateAsync(T entity, CancellationToken ct = default)
        {
            // Вызываем метод обновления из твоего репозитория
            return await _repository.UpdateAsync(entity, ct);
        }

    }
    public class AuthService : IAuthService
    {
        private const int CodeLifetimeMinutes = 10;
        private const int MaxAttempts = 5;
        private const int ResendCooldownSeconds = 60;
        private const int MaxCodesPerHour = 5;
        private int TokenLifetimeDays => int.TryParse(_config["Jwt:LifetimeDays"], out var days) ? days : 7;

        private readonly AppDbContext _db;
        private readonly IEmailSender _email;
        private readonly IConfiguration _config;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            AppDbContext db,
            IEmailSender email,
            IConfiguration config,
            ILogger<AuthService> logger)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _email = email ?? throw new ArgumentNullException(nameof(email));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // ---------- Вход по коду из письма ----------

        public async Task SendCodeAsync(string rawEmail, CancellationToken ct = default)
        {
            var email = NormalizeEmail(rawEmail);
            var now = DateTime.UtcNow;

            var last = await _db.EmailCodes
                .Where(c => c.Email == email)
                .OrderByDescending(c => c.CreatedAt)
                .FirstOrDefaultAsync(ct);

            if (last != null && last.CreatedAt > now.AddSeconds(-ResendCooldownSeconds))
                throw new AuthException("Подождите минуту перед повторной отправкой кода", 429);

            var sentLastHour = await _db.EmailCodes
                .CountAsync(c => c.Email == email && c.CreatedAt > now.AddHours(-1), ct);

            if (sentLastHour >= MaxCodesPerHour)
                throw new AuthException("Слишком много запросов. Попробуйте позже", 429);

            var code = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();

            var entity = new EmailCode
            {
                Email = email,
                CodeHash = HashCode(email, code),
                ExpiresAt = now.AddMinutes(CodeLifetimeMinutes)
            };
            _db.EmailCodes.Add(entity);
            await _db.SaveChangesAsync(ct);

            try
            {
                await _email.SendAsync(
                    email,
                    "Код входа в FoodRecipe",
                    $"Ваш код: {code}\n\nОн действует {CodeLifetimeMinutes} минут. " +
                    "Если вы не запрашивали код, просто проигнорируйте это письмо.",
                    ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Не удалось отправить письмо на {Email}", email);

                _db.EmailCodes.Remove(entity);
                await _db.SaveChangesAsync(CancellationToken.None);

                throw new AuthException("Не удалось отправить письмо. Попробуйте позже", 503);
            }
        }

        public async Task<AuthResultDto> VerifyCodeAsync(string rawEmail, string code, CancellationToken ct = default)
        {
            var email = NormalizeEmail(rawEmail);
            code = (code ?? "").Trim();
            var now = DateTime.UtcNow;

            var entry = await _db.EmailCodes
                .Where(c => c.Email == email && !c.Used && c.ExpiresAt > now)
                .OrderByDescending(c => c.CreatedAt)
                .FirstOrDefaultAsync(ct);

            if (entry == null)
                throw new AuthException("Код недействителен или истёк. Запросите новый", 401);

            if (entry.Attempts >= MaxAttempts)
                throw new AuthException("Слишком много попыток. Запросите новый код", 429);

            var isValid = CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(entry.CodeHash),
                Encoding.UTF8.GetBytes(HashCode(email, code)));

            if (!isValid)
            {
                entry.Attempts++;
                await _db.SaveChangesAsync(ct);
                throw new AuthException("Неверный код", 401);
            }

            // Код подошёл: гасим все неиспользованные коды этого email
            await _db.EmailCodes
                .Where(c => c.Email == email && !c.Used)
                .ExecuteUpdateAsync(s => s.SetProperty(c => c.Used, true), ct);

            var user = await GetOrCreateUserAsync(email, ct);
            return BuildResult(user);
        }

        // ---------- Вход через Google ----------

        public async Task<AuthResultDto> GoogleAsync(string idToken, CancellationToken ct = default)
        {
            var clientId = _config["Google:ClientId"];
            if (string.IsNullOrWhiteSpace(clientId))
                throw new AuthException("Вход через Google не настроен", 503);

            GoogleJsonWebSignature.Payload payload;
            try
            {
                payload = await GoogleJsonWebSignature.ValidateAsync(
                    idToken,
                    new GoogleJsonWebSignature.ValidationSettings
                    {
                        Audience = new[] { clientId }
                    });
            }
            catch (InvalidJwtException)
            {
                throw new AuthException("Недействительный токен Google", 401);
            }

            if (!payload.EmailVerified || string.IsNullOrWhiteSpace(payload.Email))
                throw new AuthException("Email в аккаунте Google не подтверждён", 401);

            var email = NormalizeEmail(payload.Email);

            // 1) уже входил через Google
            var login = await _db.UserLogins
                .Include(l => l.User)
                .FirstOrDefaultAsync(l => l.Provider == "Google" && l.ProviderKey == payload.Subject, ct);

            // 2) иначе ищем по email или создаём нового пользователя
            var user = login?.User ?? await GetOrCreateUserAsync(email, ct);

            if (user.IsDeleted)
                throw new AuthException("Аккаунт отключён", 403);

            if (login == null)
            {
                _db.UserLogins.Add(new UserLogin
                {
                    UserId = user.Id,
                    Provider = "Google",
                    ProviderKey = payload.Subject
                });
                await _db.SaveChangesAsync(ct);
            }

            return BuildResult(user);
        }

        // ---------- Профиль ----------

        public async Task<UserDto> GetMeAsync(Guid userId, CancellationToken ct = default)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted, ct)
                       ?? throw new AuthException("Пользователь не найден", 401);

            return ToDto(user);
        }

        public async Task<UserDto> SetNameAsync(Guid userId, string rawName, CancellationToken ct = default)
        {
            var name = (rawName ?? "").Trim();
            if (name.Length < 2 || name.Length > 50)
                throw new AuthException("Имя должно быть от 2 до 50 символов");

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted, ct)
                       ?? throw new AuthException("Пользователь не найден", 401);

            user.Name = name;
            user.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);

            return ToDto(user);
        }

        // ---------- Вспомогательное ----------

        private async Task<User> GetOrCreateUserAsync(string email, CancellationToken ct)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email, ct);

            if (user == null)
            {
                user = new User { Email = email, EmailConfirmed = true, Role = UserRole.User };
                _db.Users.Add(user);

                try
                {
                    await _db.SaveChangesAsync(ct);
                }
                catch (DbUpdateException)
                {
                    // два запроса одновременно создали одного пользователя: берём уже созданного
                    _db.Entry(user).State = EntityState.Detached;
                    user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email, ct)
                           ?? throw new AuthException("Не удалось создать пользователя", 500);
                }
            }

            if (user.IsDeleted)
                throw new AuthException("Аккаунт отключён", 403);

            if (!user.EmailConfirmed)
            {
                user.EmailConfirmed = true;
                user.UpdatedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync(ct);
            }

            return user;
        }

        private static string NormalizeEmail(string? raw)
        {
            var email = (raw ?? "").Trim().ToLowerInvariant();

            if (email.Length == 0 || email.Length > 254 ||
                !MailAddress.TryCreate(email, out var parsed) || parsed.Address != email)
            {
                throw new AuthException("Некорректный email");
            }

            return email;
        }

        // Хеш кода с секретом сервера: даже при утечке базы код нельзя подобрать перебором
        private string HashCode(string email, string code)
        {
            var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]!);
            var data = Encoding.UTF8.GetBytes($"{email}:{code}");
            return Convert.ToHexString(HMACSHA256.HashData(key, data));
        }

        private AuthResultDto BuildResult(User user) => new()
        {
            AccessToken = CreateJwt(user),
            NeedsName = string.IsNullOrWhiteSpace(user.Name),
            User = ToDto(user)
        };

        private static UserDto ToDto(User u) => new()
        {
            Id = u.Id,
            Email = u.Email,
            Name = u.Name,
            Role = u.Role.ToString()
        };

        private string CreateJwt(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));

            var descriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role.ToString())
                }),
                Expires = DateTime.UtcNow.AddDays(TokenLifetimeDays),
                Issuer = _config["Jwt:Issuer"],
                Audience = _config["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            };

            var handler = new JwtSecurityTokenHandler();
            return handler.WriteToken(handler.CreateToken(descriptor));
        }
    }
    // Настоящая отправка писем через SMTP (Gmail: smtp.gmail.com:587 + пароль приложения)
    public class SmtpEmailSender : IEmailSender
    {
        private readonly IConfiguration _config;

        public SmtpEmailSender(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public async Task SendAsync(string to, string subject, string body, CancellationToken ct = default)
        {
            var host = _config["Email:Host"] ?? "smtp.gmail.com";
            var port = int.Parse(_config["Email:Port"] ?? "587");
            var user = _config["Email:User"] ?? throw new InvalidOperationException("Не задан Email:User");
            var password = _config["Email:Password"] ?? throw new InvalidOperationException("Не задан Email:Password");
            var fromName = _config["Email:FromName"] ?? "FoodRecipe";

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName, user));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;
            message.Body = new TextPart("plain") { Text = body };

            using var client = new MailKit.Net.Smtp.SmtpClient();
            client.CheckCertificateRevocation = false;
            await client.ConnectAsync(host, port, MailKit.Security.SecureSocketOptions.StartTls, ct);
            await client.AuthenticateAsync(user, password, ct);
            await client.SendAsync(message, ct);
            await client.DisconnectAsync(true, ct);
        }
    }
    // Только для разработки: вместо отправки письма пишет его в консоль
    public class ConsoleEmailSender : IEmailSender
    {
        private readonly ILogger<ConsoleEmailSender> _logger;

        public ConsoleEmailSender(ILogger<ConsoleEmailSender> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task SendAsync(string to, string subject, string body, CancellationToken ct = default)
        {
            _logger.LogWarning("[DEV EMAIL] To: {To} | Subject: {Subject}\n{Body}", to, subject, body);
            return Task.CompletedTask;
        }
    }
}
