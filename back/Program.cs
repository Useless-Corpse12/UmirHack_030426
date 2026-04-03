using System.Text;
using DeliveryAggregator.Data;
using DeliveryAggregator.Hubs;
using DeliveryAggregator.Middleware;
using DeliveryAggregator.Repositories.Implementations;
using DeliveryAggregator.Repositories.Interfaces;
using DeliveryAggregator.Repositories.Mock;
using DeliveryAggregator.Services;
using DeliveryAggregator.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .WithOrigins("http://localhost:3000", "http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var jwtKey = builder.Configuration["Jwt:Key"]!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                    context.Token = accessToken;
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// Сервисы (одинаковые независимо от БД)
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IApplicationService, ApplicationService>();
builder.Services.AddScoped<IOrganizationService, OrganizationService>();
builder.Services.AddScoped<IMenuService, MenuService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ICourierService, CourierService>();
builder.Services.AddScoped<IModeratorService, ModeratorService>();

// Проверяем БД и регистрируем реальные или mock репозитории
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;
var dbAvailable = CheckDatabaseAvailable(connectionString);

if (dbAvailable)
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("✓ База данных доступна — используются реальные репозитории");
    Console.ResetColor();

    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(connectionString));

    builder.Services.AddScoped<IUserRepository, UserRepository>();
    builder.Services.AddScoped<IApplicationRepository, ApplicationRepository>();
    builder.Services.AddScoped<IOrganizationRepository, OrganizationRepository>();
    builder.Services.AddScoped<IRestaurantRepository, RestaurantRepository>();
    builder.Services.AddScoped<IMenuRepository, MenuRepository>();
    builder.Services.AddScoped<ICourierRepository, CourierRepository>();
    builder.Services.AddScoped<IOrderRepository, OrderRepository>();
}
else
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("⚠ ВНИМАНИЕ: База данных недоступна!");
    Console.WriteLine("⚠ Режим MOCK — тестовые данные в памяти, сбрасываются при перезапуске.");
    Console.WriteLine("");
    Console.WriteLine("  Тестовые аккаунты:");
    Console.WriteLine("  moderator@test.com  / Admin123    (Модератор)");
    Console.WriteLine("  owner@dodopizza.ru  / Owner123    (Владелец орги)");
    Console.WriteLine("  courier@test.com    / Courier123  (Курьер)");
    Console.WriteLine("  customer@test.com   / Customer123 (Покупатель)");
    Console.ResetColor();

    // Singleton — данные живут пока работает приложение
    builder.Services.AddSingleton<IUserRepository, MockUserRepository>();
    builder.Services.AddSingleton<IApplicationRepository, MockApplicationRepository>();
    builder.Services.AddSingleton<IOrganizationRepository, MockOrganizationRepository>();
    builder.Services.AddSingleton<IRestaurantRepository, MockRestaurantRepository>();
    builder.Services.AddSingleton<IMenuRepository, MockMenuRepository>();
    builder.Services.AddSingleton<ICourierRepository, MockCourierRepository>();
    builder.Services.AddSingleton<IOrderRepository, MockOrderRepository>();
}

var app = builder.Build();

if (dbAvailable)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.UseMiddleware<ExceptionMiddleware>();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<OrderHub>("/hubs/orders");

app.Run();

static bool CheckDatabaseAvailable(string connectionString)
{
    try
    {
        using var conn = new NpgsqlConnection(connectionString);
        conn.Open();
        return true;
    }
    catch
    {
        return false;
    }
}
