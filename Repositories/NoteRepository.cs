using LineNoteBot.Data;
using LineNoteBot.Models.Entities;
using LineNoteBot.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LineNoteBot.Repositories;

public class NoteRepository(AppDbContext db) : INoteRepository
{
    public async Task AddAsync(Note note)
    {
        db.Notes.Add(note);
        await db.SaveChangesAsync();
    }

    public async Task<IEnumerable<Note>> SearchAsync(string keyword)
    {
        var normalizedKeyword = keyword.Trim().Replace(" ", "").ToLower();

        var result = await db.Notes
            .AsNoTracking()
            .Where(n =>
                n.Content != null &&
                n.Content
                 .Replace(" ", "")
                 .ToLower()
                 .Contains(normalizedKeyword))
            .ToListAsync();

        return result;
    }

    public async Task<IEnumerable<Note>> GetRecentAsync(int count = 5)
    {
        var result = await db.Notes
            .AsNoTracking()
            .OrderByDescending(n => n.CreatedAt)
            .Take(count)
            .ToListAsync();

        return result;
    }
}
