using FoodRecipe.Data;
using FoodRecipe.Repositories;
using FoodRecipe.Services.IService;
using FoodRecipe.Services.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using WebApplication1.Repositories;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// ---------- JWT: ключ берётся из user-secrets, а не из кода ----------
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Не задан Jwt:Key. Добавьте его в user-secrets.");
if (jwtKey.Length < 32)
    throw new InvalidOperationException("Jwt:Key слишком короткий (нужно минимум 32 символа).");

var jwtIssuer = builder.Configuration["Jwt:Issuer"]!;
var jwtAudience = builder.Configuration["Jwt:Audience"]!;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),

            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,

            ValidateAudience = true,
            ValidAudience = jwtAudience,

            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

// ---------- CORS: какому сайту разрешено обращаться к API ----------
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                     ?? Array.Empty<string>();

builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()));

// ---------- Ограничение частоты запросов для /api/auth ----------
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("auth", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
});

// ---------- База данных ----------
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("FoodRecipe"))
    .LogTo(Console.Write, LogLevel.Information)
       .UseSnakeCaseNamingConvention());

// ---------- Swagger ----------
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "FoodRecipe", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Вставьте JWT токен (без слова Bearer)",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// ---------- Сервисы ----------
builder.Services.AddScoped(typeof(IBaseService<>), typeof(BaseService<>));
builder.Services.AddScoped(typeof(IPostgreSQLRepository<>), typeof(PostgreSQLRepository<>));

builder.Services.AddScoped<IAuthService, AuthService>();

// Отправка писем: настоящая, если задан Email:Password; в разработке иначе пишем код в консоль
if (!string.IsNullOrWhiteSpace(builder.Configuration["Email:Password"]))
{
    builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();
}
else if (builder.Environment.IsDevelopment())
{
    builder.Services.AddScoped<IEmailSender, ConsoleEmailSender>();
}
else
{
    throw new InvalidOperationException("Не настроена отправка почты (Email:User и Email:Password).");
}

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);
var app = builder.Build();

// Автоматическое применение миграций при старте
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        context.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ошибка при автоматическом применении миграций.");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.EnablePersistAuthorization();
    });
}

app.UseHttpsRedirection();

// ПОРЯДОК ВАЖЕН:
app.UseCors();
app.UseRateLimiter();
app.UseAuthentication(); // 1. Проверяем токен
app.UseAuthorization();  // 2. Проверяем роль

app.MapControllers();

app.Run();