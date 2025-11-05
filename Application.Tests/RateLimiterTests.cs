using System;
using Application.Services;
using FluentAssertions;
using Xunit;

namespace Application.Tests
{
    public class RateLimiterTests
    {
        [Fact]
        public void Allows_within_limit_and_blocks_after_limit()
        {
            var limiter = new RateLimiter(3, TimeSpan.FromSeconds(1));
            var key = "user@example.com";

            limiter.TryConsume(key).Should().BeTrue();
            limiter.TryConsume(key).Should().BeTrue();
            limiter.TryConsume(key).Should().BeTrue();
            // fourth should be blocked
            limiter.TryConsume(key).Should().BeFalse();
        }
    }
}
