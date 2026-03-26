namespace NexusAssistant.Api.Models;

public class AskRequest
{
    public Guid SessionId { get; set; }
    public string Message { get; set; } = string.Empty;
}