using System.Threading.Tasks;

namespace API.Services;

public interface IEmailQueue
{
    ValueTask EnqueueEmailAsync(string toEmail, string callbackUrl);
}
