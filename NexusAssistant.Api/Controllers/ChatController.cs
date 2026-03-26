using Microsoft.AspNetCore.Mvc;
using NexusAssistant.Api.Memory;
using NexusAssistant.Api.Models;
using NexusAssistant.Api.Pipeline;

namespace NexusAssistant.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly AgentPipeline _pipeline;
    private readonly ISessionStore _sessionStore;

    public ChatController(AgentPipeline pipeline, ISessionStore sessionStore)
    {
        _pipeline = pipeline;
        _sessionStore = sessionStore;
    }

    [HttpPost("ask")]
    public async Task AskStream([FromBody] AskRequest request)
    {
        Response.Headers.Append("Content-Type", "text/event-stream; charset=utf-8");
        Response.Headers.Append("Cache-Control", "no-cache");
        Response.Headers.Append("X-Accel-Buffering", "no");
        Response.ContentType = "text/event-stream; charset=utf-8";

        var writer = new StreamWriter(Response.Body, new System.Text.UTF8Encoding(false));

        await foreach (var chunk in _pipeline.RunStreamAsync(
            request.SessionId,
            request.Message))
        {
            await writer.WriteAsync($"{chunk}");
            await writer.FlushAsync();
        }
    }

    [HttpPost("session/new")]
    public IActionResult NewSession()
    {
        var sessionId = Guid.NewGuid();
        return Ok(new { sessionId });
    }

    [HttpGet("sessions")]
    public async Task<IActionResult> GetSessions()
    {
        var sessions = await _sessionStore.GetAllAsync();
        return Ok(sessions.Select(s => new
        {
            s.Id,
            s.Title,
            s.CreatedAt,
            s.LastActivity,
            s.Tags,
            s.Preview
        }));
    }

    [HttpDelete("session/{sessionId}")]
    public async Task<IActionResult> DeleteSession(Guid sessionId)
    {
        await _sessionStore.DeleteAsync(sessionId);
        return NoContent();
    }
}
