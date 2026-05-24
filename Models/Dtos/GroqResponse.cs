namespace LineNoteBot.Models.Dtos;

public class GroqResponse
{
    public List<GroqChoice> Choices { get; set; } = [];
}
public class GroqChoice
{
    public GroqMessage Message { get; set; } = new();
}
public class GroqMessage
{
    public string Content { get; set; } = string.Empty;
}
