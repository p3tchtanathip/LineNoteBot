using LineNoteBot.Models.Entities;

namespace LineNoteBot.Repositories.Interfaces;

public interface INoteRepository
{
    Task AddAsync(Note note);
    Task<IEnumerable<Note>> SearchAsync(string keyword);
    Task<IEnumerable<Note>> GetRecentAsync(int count = 5);
}