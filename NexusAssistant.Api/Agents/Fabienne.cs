using Microsoft.Extensions.Options;
using NexusAssistant.Api.Config;
using NexusAssistant.Api.Providers;

namespace NexusAssistant.Api.Agents;

public class Fabienne : Auguste
{

    protected override string SystemPrompt => """
You are Fabienne, an expert in software architecture and functional design.
You are part of a discussion group with other specialists.

Your role:
- Analyze functional and architectural requirements
- Propose suitable patterns (Repository, CQRS, ECS, MVC, MVVM...)
- Identify design risks
- Verify consistency with the existing project
- Challenge overly complex or over-engineered approaches

Your areas of expertise:
- General software architecture
- Design patterns
- Business applications (DDD, Clean Architecture)
- REST APIs (endpoint design, versioning, security)
- Architecture definition

Rules:
- Be concise and structured
- Never provide code; that's Clément's role
- If you disagree with Clément, support your argument with facts
- Answer in the language in which the question is asked.
""";

    public Fabienne(ILLMProvider provider, IOptions<NexusConfig> config)
    : base(provider, config, config.Value.Agents.Fabienne) { }

    public async Task<string> AnalyzeAsync(string userMessage, string context = "")
    {
        var history = BuildHistory(userMessage, context);
        return Clean(await CompleteAsync(history));
    }

    public async IAsyncEnumerable<string> AnalyzeStreamAsync(
        string userMessage, string context = "")
    {
        var history = BuildHistory(userMessage, context);
        await foreach (var token in StreamAsync(history))
            yield return token;
    }

    public Task<string> RespondToDebateAsync(
        string userMessage,
        string clementResponse,
        string context = "")
        => RespondToDebateAsync(
            userMessage,
            "Clément",
            clementResponse,
            () => AnalyzeAsync(userMessage, context),
            context);
}
