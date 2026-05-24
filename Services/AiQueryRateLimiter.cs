using LineNoteBot.Services.Interfaces;
using Microsoft.Extensions.Caching.Distributed;

namespace LineNoteBot.Services;

public class AiQueryRateLimiter(IDistributedCache cache, ILogger<AiQueryRateLimiter> logger) : IAiQueryRateLimiter
{
    private const int DailyLimit = 20;

    public async Task<bool> TryConsumeAsync(string userId)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow).ToString("yyyyMMdd");
        var key = $"ai_limit:{userId}:{today}";

        var countString = await cache.GetStringAsync(key);
        var currentCount = int.TryParse(countString, out var value) ? value : 0;

        if (currentCount >= DailyLimit)
        {
            logger.LogWarning("User {UserId} exceeded AI limit", userId);
            return false;
        }

        var tomorrow = DateTime.UtcNow.Date.AddDays(1);

        await cache.SetStringAsync(
            key,
            (currentCount + 1).ToString(),
            new DistributedCacheEntryOptions { AbsoluteExpiration = tomorrow });

        return true;
    }
}
