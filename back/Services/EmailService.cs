using System.Net;
using System.Net.Mail;
using DeliveryAggregator.Services.Interfaces;

namespace DeliveryAggregator.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration config, ILogger<EmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendConfirmationEmailAsync(string toEmail, string displayName, string token)
    {
        var baseUrl = _config["App:BaseUrl"] ?? "http://localhost:5000";
        var confirmUrl = $"{baseUrl}/api/auth/confirm-email?token={token}";

        var body = $"""
            <h2>Добро пожаловать, {displayName}!</h2>
            <p>Для подтверждения email нажмите на кнопку ниже:</p>
            <a href="{confirmUrl}"
               style="background:#2563eb;color:white;padding:12px 24px;text-decoration:none;border-radius:6px;display:inline-block">
               Подтвердить email
            </a>
            <p>Или перейдите по ссылке: <a href="{confirmUrl}">{confirmUrl}</a></p>
            <p>Ссылка действительна 24 часа.</p>
            """;

        await SendEmailAsync(toEmail, "Подтверждение email — Delivery Aggregator", body);
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        var host     = _config["Smtp:Host"];
        var port     = int.Parse(_config["Smtp:Port"] ?? "587");
        var user     = _config["Smtp:User"];
        var password = _config["Smtp:Password"];
        var fromName = _config["Smtp:FromName"] ?? "Delivery Aggregator";

        if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(user))
        {
            // SMTP не настроен — просто логируем (режим разработки)
            _logger.LogWarning("SMTP не настроен. Письмо для {Email}: {Subject}", toEmail, subject);
            _logger.LogWarning("Содержимое: {Body}", body);
            return;
        }

        try
        {
            using var client = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(user, password),
                EnableSsl = true
            };

            var message = new MailMessage
            {
                From = new MailAddress(user, fromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            message.To.Add(toEmail);

            await client.SendMailAsync(message);
            _logger.LogInformation("Письмо отправлено на {Email}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка отправки письма на {Email}", toEmail);
            // Не бросаем исключение — почта не должна ломать регистрацию
        }
    }
}
