using System.Threading;
using System.Threading.Tasks;
using ExchangeMonitor.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MailKit.Net.Smtp;
using MimeKit;

namespace ExchangeMonitor.Infrastructure.ExternalServices;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendAlertEmailAsync(string subject, string body, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Sending email alert via Mailtrap: {Subject}", subject);

        var host = _configuration["Mailtrap:Host"];
        var port = int.Parse(_configuration["Mailtrap:Port"] ?? "587");
        var username = _configuration["Mailtrap:Username"];
        var password = _configuration["Mailtrap:Password"];

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("Exchange Monitor", "alerts@exchangemonitor.com"));
        message.To.Add(new MailboxAddress("User", "user@example.com"));
        message.Subject = subject;

        message.Body = new TextPart("html")
        {
            Text = $"<h3>Exchange Rate Alert</h3><p>{body}</p>"
        };

        using var client = new SmtpClient();
        try
        {
            await client.ConnectAsync(host, port, MailKit.Security.SecureSocketOptions.StartTls, cancellationToken);
            await client.AuthenticateAsync(username, password, cancellationToken);
            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);
            
            _logger.LogInformation("Email sent successfully to Mailtrap!");
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Failed to send email via Mailtrap.");
            throw;
        }
    }
}
