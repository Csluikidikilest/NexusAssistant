using Microsoft.Extensions.Options;
using NexusAssistant.Api.Config;
using NexusAssistant.Api.Memory;
using NexusAssistant.Api.Models;
using NexusAssistant.Api.Providers;

namespace NexusAssistant.Api.Agents;

public class Mauricette : Auguste
{
    private readonly ISessionStore _sessionStore;

    protected override string SystemPrompt => """
        Tu es Mauricette, gardienne de la mémoire du groupe Nexus.
        
        Ton rôle :
        - Résumer les échanges importants pour les mémoriser
        - Extraire les informations clés d'une discussion
          (technologies utilisées, décisions prises, patterns choisis)
        - Fournir un contexte pertinent avant chaque nouvelle question
        - Identifier les tags pertinents pour classifier les sessions
        
        Règles :
        - Sois factuelle et concise
        - Priorise les décisions techniques et architecturales
        - Ignore les échanges sans valeur informative
        """;

    public Mauricette(
    ILLMProvider provider,
    IOptions<NexusConfig> config,
    ISessionStore sessionStore)
    : base(provider, config, config.Value.Agents.Mauricette)
    {
        _sessionStore = sessionStore;
    }

    public async Task<string> RememberAsync(Guid sessionId, string userMessage)
    {
        var session = await _sessionStore.GetOrCreateAsync(sessionId);

        if (!session.Messages.Any())
            return string.Empty;

        var relatedSessions = await _sessionStore.SearchAsync(userMessage);

        var recentMessages = session.Messages
            .TakeLast(10)
            .Select(m => $"[{m.Role}] {m.Content}")
            .ToList();

        var relatedContext = relatedSessions
            .Where(s => s.Id != sessionId)
            .Take(2)
            .SelectMany(s => s.Messages.TakeLast(5))
            .Select(m => $"[{m.Role}] {m.Content}")
            .ToList();

        var context = new List<string>();

        if (recentMessages.Any())
            context.Add(
                $"Discussion en cours :\n{string.Join("\n", recentMessages)}");

        if (relatedContext.Any())
            context.Add(
                $"Sessions similaires :\n{string.Join("\n", relatedContext)}");

        return string.Join("\n\n", context);
    }

    public async Task LearnAsync(
        Guid sessionId,
        string userMessage,
        string oscarResponse)
    {
        var session = await _sessionStore.GetOrCreateAsync(sessionId);

        session.AddMessage(MessageRole.User, userMessage);
        session.AddMessage(MessageRole.Oscar, oscarResponse);

        if (session.Messages.Count % 10 == 0)
            await SummarizeSessionAsync(session);

        if (session.Messages.Count == 2)
            session.Title = userMessage[..Math.Min(50, userMessage.Length)];

        await _sessionStore.SaveAsync(session);
    }

    private async Task SummarizeSessionAsync(NexusSession session)
    {
        var messages = session.Messages
            .Select(m => $"[{m.Role}] {m.Content}")
            .ToList();

        var history = BuildHistory($"""
            Résume les points clés de cette discussion en 5 bullet points maximum.
            Focus sur les décisions techniques et architecturales.
            
            Discussion :
            {string.Join("\n", messages)}
            """);

        var response = await CompleteAsync(history);

        if (!string.IsNullOrEmpty(response))
            session.Tags.Add($"Résumé : {response}");
    }
}