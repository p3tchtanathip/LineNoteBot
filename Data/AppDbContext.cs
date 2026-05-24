using LineNoteBot.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace LineNoteBot.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Note> Notes => Set<Note>();
}
