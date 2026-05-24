using LineNoteBot.Models.Entities;
using LineNoteBot.Repositories.Interfaces;
using LineNoteBot.Services.Interfaces;

namespace LineNoteBot.Services
{
    public class NoteService(INoteRepository repo, IAiService ai, IAiQueryRateLimiter limiter) : INoteService
    {
        public async Task<string> HandleMessageAsync(string text, string userId)
        {
            if (text.StartsWith("search "))
            {
                var keyword = text[7..];
                var results = await repo.SearchAsync(keyword);
                return results.Any()
                    ? string.Join("\n", results.Select(n => $"• {n.Content}"))
                    : "No matching notes found";
            }
            if (text == "latest")
            {
                var notes = await repo.GetRecentAsync(5);
                return string.Join("\n", notes.Select((n, i) => $"{i + 1}. {n.Content}"));
            }
            if (text.StartsWith("ask "))
            {
                var canUseAi = await limiter.TryConsumeAsync(userId);
                if (!canUseAi) return "You've reached the daily AI limit (20/day). Try again tomorrow.";

                var question = text[4..];
                var allNotes = await repo.SearchAsync("");
                return await ai.AskAsync(question, allNotes.Select(n => n.Content)) ?? "I couldn't find enough information in your notes.";
            }

            await repo.AddAsync(new Note { Content = text });
            return "Note saved";
        }
    }
}