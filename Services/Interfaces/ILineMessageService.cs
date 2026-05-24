namespace LineNoteBot.Services.Interfaces;

public interface ILineMessageService
{
    Task ReplyAsync(string replyToken, string message);
    Task ReplyMultipleAsync(string replyToken, IEnumerable<string> messages);
}
