using System.Collections.Concurrent;

namespace Application.Services;

/// <summary>
/// Simple in-memory sliding-window rate limiter for unit tests and basic usage.
/// Not distributed — use a shared store in production.
/// </summary>
public class RateLimiter
{
    private readonly ConcurrentDictionary<string, (int Count, DateTime WindowStart)> _store = new();
    private readonly TimeSpan _window;
    private readonly int _limit;

    public RateLimiter(int limit, TimeSpan window)
    {
        _limit = limit;
        _window = window;
    }

    public bool TryConsume(string key)
    {
        var now = DateTime.UtcNow;
        _store.AddOrUpdate(key,
            _ => (1, now),
            (_, state) =>
            {
                if (now - state.WindowStart > _window)
                {
                    return (1, now);
                }
                return (state.Count + 1, state.WindowStart);
            });

        var current = _store[key];
        return current.Count <= _limit;
    }
}
