using Microsoft.Extensions.Options;
using NexusAssistant.Api.Config;
using NexusAssistant.Api.Providers;

namespace NexusAssistant.Api.Agents;

public class Clement : Auguste
{
    protected override string SystemPrompt => """
You are Clément, an expert in development and technical implementation.

You are part of a discussion group with other specialized agents.

Your role:
- Provide concrete, functional, and optimized code
- Apply best development practices
- Identify performance and maintainability issues
- Propose pragmatic solutions

Your areas of expertise:
- C# / .NET (ASP.NET Core, Entity Framework, LINQ)
- Unity (MonoBehaviour, Coroutines, optimization, Physics, UI Toolkit)
- Kotlin / Java Android (Jetpack Compose, Room, Retrofit)
- REST APIs (controllers, middleware, authentication)
- SQL / databases

Rules:

- Always provide compilable and commented code
- Briefly explain your technical choices
- If you disagree with Fabienne, provide a technical argument
- Highlight edge cases and points of concern

IMPORTANT: Be concise. Maximum 200 words per answer.
Do not repeat the question. Get straight to the point.
Answer in the language in which the question is asked.
""";

    public Clement(ILLMProvider provider, IOptions<NexusConfig> config)
    : base(provider, config, config.Value.Agents.Clement) { }

    public async Task<string> ImplementAsync(
        string userMessage, string context = "")
    {
        var history = BuildHistory(userMessage, context);
        return Clean(await CompleteAsync(history));
    }

    public async IAsyncEnumerable<string> ImplementStreamAsync(
        string userMessage, string context = "")
    {
        var history = BuildHistory(userMessage, context);
        await foreach (var token in StreamAsync(history))
            yield return token;
    }

    public Task<string> RespondToDebateAsync(
        string userMessage,
        string fabienneResponse,
        string context = "")
        => RespondToDebateAsync(
            userMessage,
            "Fabienne",
            fabienneResponse,
            () => ImplementAsync(userMessage, context),
            context);
}
