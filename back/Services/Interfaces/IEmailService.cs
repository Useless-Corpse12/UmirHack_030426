namespace DeliveryAggregator.Services.Interfaces;

public interface IEmailService
{
    Task SendConfirmationEmailAsync(string toEmail, string displayName, string token);
    Task SendEmailAsync(string toEmail, string subject, string body);
}
