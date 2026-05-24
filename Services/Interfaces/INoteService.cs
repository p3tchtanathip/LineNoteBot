namespace LineNoteBot.Services.Interfaces;

public interface INoteService
{
    Task<string> HandleMessageAsync(string text, string userId);
}
