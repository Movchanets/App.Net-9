using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace API.Services;

public class SmtpEmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IConfiguration configuration, ILogger<SmtpEmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendPasswordResetEmailAsync(string toEmail, string callbackUrl)
    {
        var section = _configuration.GetSection("SmtpSettings");
        var host = section.GetValue<string>("Host");
        var port = section.GetValue<int?>("Port") ?? 25;
        var user = section.GetValue<string>("Username");
        var pass = section.GetValue<string>("Password");
        var from = section.GetValue<string>("From") ?? user ?? "noreply@example.com";
        var enableSsl = section.GetValue<bool?>("EnableSsl") ?? true;

        // Try to load HTML template if configured, otherwise use plaintext
        var templatePath = section.GetValue<string>("TemplatePath");
        string body;
        bool isHtml = false;
        if (!string.IsNullOrEmpty(templatePath) && System.IO.File.Exists(templatePath))
        {
            body = System.IO.File.ReadAllText(templatePath);
            body = body.Replace("{{CallbackUrl}}", callbackUrl);
            body = body.Replace("{{Email}}", toEmail);
            isHtml = true;
        }
        else
        {
            body = $"Please use the following link to reset your password:\n\n{callbackUrl}\n\nIf you didn't request this, ignore this email.";
        }

        var fromName = section.GetValue<string>("FromName") ?? from;
        var fromAddress = new MailAddress(from, fromName);

        using var message = new MailMessage();
        message.From = fromAddress;
        message.To.Add(toEmail);
        message.Subject = "Password reset";
        message.Body = body;
        message.IsBodyHtml = isHtml;

        using var client = new SmtpClient(host, port)
        {
            EnableSsl = enableSsl,
        };

        if (!string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(pass))
        {
            client.Credentials = new NetworkCredential(user, pass);
        }

        try
        {
            _logger.LogInformation("Sending password reset email to {Email} via SMTP host {Host}", toEmail, host);
            await client.SendMailAsync(message);
            _logger.LogInformation("Password reset email sent to {Email}", toEmail);
        }
        catch (System.Exception ex)
        {
            // Log without exposing sensitive token content
            _logger.LogError(ex, "Failed to send password reset email to {Email}", toEmail);
            throw;
        }
    }
}
