namespace LineNoteBot.Models.Dtos;

public class LineWebhookRequest
{
    public string Destination { get; set; } = string.Empty;
    public List<LineEvent> Events { get; set; } = [];
}

public class LineEvent
{
    public string Type { get; set; } = string.Empty;
    public string ReplyToken { get; set; } = string.Empty;
    public LineSource Source { get; set; } = new();
    public long Timestamp { get; set; }
    public string Mode { get; set; } = string.Empty;
    public LineMessage? Message { get; set; }
}

public class LineSource
{
    public string Type { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string? GroupId { get; set; }
    public string? RoomId { get; set; }
}

public class LineMessage
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? Text { get; set; }
}