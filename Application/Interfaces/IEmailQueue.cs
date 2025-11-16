namespace Application.Interfaces;

public interface IEmailQueue
{
    ValueTask EnqueueEmailAsync(string toEmail, string callbackUrl);
}
