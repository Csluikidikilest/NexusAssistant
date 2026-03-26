namespace NexusAssistant.Api.Models;

public class NexusMessage
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public MessageRole Role { get; init; }
    public string Content { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}