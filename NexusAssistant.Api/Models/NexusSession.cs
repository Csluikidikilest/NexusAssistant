namespace NexusAssistant.Api.Models;

public class NexusSession
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Title { get; set; } = "Nouvelle discussion";
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime LastActivity { get; set; } = DateTime.UtcNow;
    public List<NexusMessage> Messages { get; init; } = [];
    public List<string> Tags { get; set; } = [];

    // Ajoute un message et met à jour l'activité
    public void AddMessage(MessageRole role, string content)
    {
        Messages.Add(new NexusMessage
        {
            Role = role,
            Content = content
        });
        LastActivity = DateTime.UtcNow;
    }

    // Résumé court pour l'affichage dans VS Code
    public string Preview => Messages
        .FirstOrDefault(m => m.Role == MessageRole.User)
        ?.Content[..Math.Min(80,
            Messages.First(m => m.Role == MessageRole.User).Content.Length)]
        ?? string.Empty;
}