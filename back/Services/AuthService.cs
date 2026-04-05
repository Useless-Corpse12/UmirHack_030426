using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DeliveryAggregator.DTOs;
using DeliveryAggregator.Entities;
using DeliveryAggregator.Enums;
using DeliveryAggregator.Repositories.Interfaces;
using DeliveryAggregator.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace DeliveryAggregator.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly IEmailConfirmationTokenRepository _emailTokens;
    private readonly IEmailService _emailService;
    private readonly IEncryptionService _encryption;
    private readonly IConfiguration _config;

    public AuthService(
        IUserRepository users,
        IEmailConfirmationTokenRepository emailTokens,
        IEmailService emailService,
        IEncryptionService encryption,
        IConfiguration config)
    {
        _users = users;
        _emailTokens = emailTokens;
        _emailService = emailService;
        _encryption = encryption;
        _config = config;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var user = await _users.GetByEmailAsync(request.Email)
            ?? throw new UnauthorizedAccessException("Неверный email или пароль");

        // Проверка soft delete — уволенный/удалённый пользователь
        if (user.IsDeleted)
            throw new UnauthorizedAccessException("Аккаунт удалён");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("Аккаунт заблокирован");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Неверный email или пароль");

        var token = GenerateToken(user);
        return new LoginResponse(token, user.Role.ToString(), user.Id, user.DisplayName, user.IsEmailConfirmed);
    }

    public async Task<LoginResponse> RegisterCustomerAsync(RegisterCustomerRequest request)
    {
        // Проверяем blacklist — даже удалённые пользователи
        var existing = await _users.GetByEmailAsync(request.Email);
        if (existing != null)
        {
            if (existing.IsDeleted)
                throw new InvalidOperationException("Этот email заблокирован");
            throw new InvalidOperationException("Email уже используется");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            DisplayName = request.DisplayName,
            // Шифруем ContactInfo (телефон)
            ContactInfo = string.IsNullOrEmpty(request.ContactInfo)
                ? null
                : _encryption.Encrypt(request.ContactInfo),
            Role = UserRole.Customer,
            IsEmailConfirmed = false
        };

        await _users.AddAsync(user);

        // Отправляем письмо подтверждения
        var confirmToken = await CreateConfirmationTokenAsync(user.Id);
        await _emailService.SendConfirmationEmailAsync(user.Email, user.DisplayName, confirmToken);

        var token = GenerateToken(user);
        return new LoginResponse(token, user.Role.ToString(), user.Id, user.DisplayName, false);
    }

    public async Task<bool> ConfirmEmailAsync(string token)
    {
        var record = await _emailTokens.GetByTokenAsync(token);

        if (record == null || record.IsUsed || record.ExpiresAt < DateTime.UtcNow)
            return false;

        var user = await _users.GetByIdAsync(record.UserId);
        if (user == null) return false;

        user.IsEmailConfirmed = true;
        await _users.UpdateAsync(user);

        record.IsUsed = true;
        await _emailTokens.UpdateAsync(record);

        return true;
    }

    public async Task ResendConfirmationAsync(string email)
    {
        var user = await _users.GetByEmailAsync(email)
            ?? throw new KeyNotFoundException("Пользователь не найден");

        if (user.IsEmailConfirmed)
            throw new InvalidOperationException("Email уже подтверждён");

        // Инвалидируем старый токен если есть
        var oldToken = await _emailTokens.GetActiveByUserIdAsync(user.Id);
        if (oldToken != null)
        {
            oldToken.IsUsed = true;
            await _emailTokens.UpdateAsync(oldToken);
        }

        var confirmToken = await CreateConfirmationTokenAsync(user.Id);
        await _emailService.SendConfirmationEmailAsync(user.Email, user.DisplayName, confirmToken);
    }

    public async Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request)
    {
        var user = await _users.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException("Пользователь не найден");

        if (!BCrypt.Net.BCrypt.Verify(request.OldPassword, user.PasswordHash))
            throw new UnauthorizedAccessException("Старый пароль неверный");

        if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 6)
            throw new InvalidOperationException("Новый пароль должен быть не менее 6 символов");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        await _users.UpdateAsync(user);
    }

    private async Task<string> CreateConfirmationTokenAsync(Guid userId)
    {
        var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
            .Replace("/", "_").Replace("+", "-").Replace("=", "");

        var record = new EmailConfirmationToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddHours(24)
        };

        await _emailTokens.AddAsync(record);
        return token;
    }

    private string GenerateToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("displayName", user.DisplayName)
        };

        var expiration = DateTime.UtcNow.AddHours(
            double.Parse(_config["Jwt:ExpirationHours"] ?? "24"));

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: expiration,
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
