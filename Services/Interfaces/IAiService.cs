using LineNoteBot.Models.Entities;

namespace LineNoteBot.Services.Interfaces;

public interface IAiService
{
    Task<string?> AskAsync(string question, IEnumerable<string> content);
}
