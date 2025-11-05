using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Application.Interfaces;

namespace API.Services;

public class EmailSenderBackgroundService : BackgroundService
{
    private readonly BackgroundEmailQueue _queue;
    private readonly IEmailService _emailService;
    private readonly ILogger<EmailSenderBackgroundService> _logger;

    public EmailSenderBackgroundService(BackgroundEmailQueue queue, IEmailService emailService, ILogger<EmailSenderBackgroundService> logger)
    {
        _queue = queue;
        _emailService = emailService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var item in _queue.ReadAllAsync().WithCancellation(stoppingToken))
        {
            try
            {
                _logger.LogInformation("Dequeued email to {Email}", item.To);
                await _emailService.SendPasswordResetEmailAsync(item.To, item.CallbackUrl);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", item.To);
                // Optionally implement retry/PoisonQueue logic here
            }
        }
    }
}
