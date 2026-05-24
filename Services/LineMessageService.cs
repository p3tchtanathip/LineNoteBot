using System.Net.Http.Headers;
using LineNoteBot.Services.Interfaces;

namespace LineNoteBot.Services;

public class LineMessageService(HttpClient http, IConfiguration config, ILogger<LineMessageService> logger) : ILineMessageService
{
    public async Task ReplyAsync(string replyToken, string message)
    {
        await ReplyMultipleAsync(replyToken, [message]);
    }

    public async Task ReplyMultipleAsync(string replyToken, IEnumerable<string> messages)
    {
        var channelAccessToken = config["Line:ChannelAccessToken"];

        if (string.IsNullOrWhiteSpace(channelAccessToken))
        {
            logger.LogError("LINE ChannelAccessToken is missing");
            throw new Exception("LINE ChannelAccessToken is missing");
        }

        const string endpoint = "bot/message/reply";

        var requestBody = new
        {
            replyToken,
            messages = messages
                .Take(5)    // LINE limit: max 5
                .Select(text => new { type = "text", text })
                .ToList()
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", channelAccessToken);
        request.Content = JsonContent.Create(requestBody);

        var response = await http.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            logger.LogError("LINE API Error: {Error}", error);
        }

        response.EnsureSuccessStatusCode();
    }
}
