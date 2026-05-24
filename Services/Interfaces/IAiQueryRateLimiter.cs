namespace LineNoteBot.Services.Interfaces;

public interface IAiQueryRateLimiter
{
    Task<bool> TryConsumeAsync(string userId);
}
