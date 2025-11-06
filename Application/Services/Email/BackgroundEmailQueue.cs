using System.Threading.Channels;

namespace Application.Services.Email;

public class BackgroundEmailQueue : IEmailQueue
{
    private readonly Channel<(string To, string CallbackUrl)> _channel;

    public BackgroundEmailQueue(int capacity = 100)
    {
        var options = new BoundedChannelOptions(capacity)
        {
            SingleReader = true,
            SingleWriter = false
        };
        _channel = Channel.CreateBounded<(string, string)>(options);
    }

    public async ValueTask EnqueueEmailAsync(string toEmail, string callbackUrl)
    {
        await _channel.Writer.WriteAsync((toEmail, callbackUrl));
    }

    internal IAsyncEnumerable<(string To, string CallbackUrl)> ReadAllAsync()
    {
        return ReadAllInternalAsync();
    }

    private async IAsyncEnumerable<(string To, string CallbackUrl)> ReadAllInternalAsync()
    {
        while (await _channel.Reader.WaitToReadAsync())
        {
            while (_channel.Reader.TryRead(out var item))
            {
                yield return item;
            }
        }
    }
}
