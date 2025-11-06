namespace Application.Services.Email;

public interface IEmailQueue
{
    ValueTask EnqueueEmailAsync(string toEmail, string callbackUrl);
}
