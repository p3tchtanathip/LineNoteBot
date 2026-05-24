using LineNoteBot.Models.Dtos;
using LineNoteBot.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LineNoteBot.controllers;

[Route("api/[controller]")]
[ApiController]
public class LineWebhookController(INoteService noteService, ILineMessageService lineService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] LineWebhookRequest request)
    {
        foreach (var ev in request.Events)
        {
            var text = ev.Message?.Text;
            var userId = ev.Source.UserId;
            if (ev.Type != "message" || ev.Message?.Type != "text" || string.IsNullOrWhiteSpace(text)) continue;

            var reply = await noteService.HandleMessageAsync(text, userId);
            await lineService.ReplyAsync(ev.ReplyToken, reply);
        }
        return Ok();
    }
}
