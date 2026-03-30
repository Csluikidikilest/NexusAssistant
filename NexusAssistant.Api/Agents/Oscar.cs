using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel.ChatCompletion;
using NexusAssistant.Api.Config;
using NexusAssistant.Api.Models;
using NexusAssistant.Api.Providers;

namespace NexusAssistant.Api.Agents;

public class Oscar : Auguste
{
    protected override string SystemPrompt => """
You are Oscar, the facilitator and moderator of the Nexus discussion group.

The group consists of Fabienne (functional), Clément (code), Eric (evaluate complexity), and Mauricette (memory).
Your role:
- Summarize Fabienne and Clément's responses
- Resolve disagreements between agents
- Produce a clear, structured, and actionable final response
- Decide if a debate between agents is necessary

Criteria for triggering a debate:
- Eric estimates the complexity is COMPLEX
- The question involves fundamental architectural choices
- Fabienne and Clément's responses contradict each other
- The question touches on several technical areas simultaneously
- The stakes in terms of performance or maintainability are high

Rules:
- Always start by asking Eric to assess ocmplexity: TRIVIAL, STANDARD or COMPLEX
- In TRIVIAL mode: try to answer directly without asking Fabienne nor Clément to intervene
- In SIMPLE mode: summarize Fabienne and Clément answers directly without debate
- In COMPLEX mode: indicate that you are initiating a debate
- The summary MUST NOT repeat what Fabienne and Clément have said.
- It must ONLY provide an actionable conclusion and the critical points of attention. Maximum 150 words.
- Answer in the language in which the question is asked.
""";

    public Oscar(ILLMProvider provider, IOptions<NexusConfig> config)
    : base(provider, config, config.Value.Agents.Oscar) { }

    // Réponse directe d'Oscar pour les questions triviales
    public async IAsyncEnumerable<string> AnswerDirectlyStreamAsync(
        string userMessage,
        string context = "")
    {
        var history = BuildHistory(userMessage, context);
        await foreach (var token in StreamAsync(history))
            yield return Clean(token);
    }

    public async Task<string> SynthesizeAsync(
        string userMessage,
        string fabienneResponse,
        string clementResponse,
        string? fabienneDebate = null,
        string? clementDebate = null,
        string context = "")
    {
        var debateSection = fabienneDebate != null && clementDebate != null
            ? $"""
               Après débat :
               Fabienne (débat) : {fabienneDebate}
               Clément (débat)  : {clementDebate}
               """
            : string.Empty;

        var history = BuildHistory($"""
            Question du développeur : {userMessage}
            
            Analyse de Fabienne : {fabienneResponse}
            Analyse de Clément  : {clementResponse}
            
            {debateSection}
            
            Produis une synthèse finale claire et actionnable.
            """, context);

        return Clean(await CompleteAsync(history));
    }

    public async IAsyncEnumerable<string> SynthesizeStreamAsync(
        string userMessage,
        string fabienneResponse,
        string clementResponse,
        string? fabienneDebate = null,
        string? clementDebate = null,
        string context = "")
    {
        var debateSection = fabienneDebate != null && clementDebate != null
            ? $"""
               Après débat :
               Fabienne (débat) : {fabienneDebate}
               Clément (débat)  : {clementDebate}
               """
            : string.Empty;

        var history = BuildHistory($"""
            Question du développeur : {userMessage}
            
            Analyse de Fabienne : {fabienneResponse}
            Analyse de Clément  : {clementResponse}
            
            {debateSection}
            
            Produis une synthèse finale claire et actionnable.
            """, context);

        await foreach (var token in StreamAsync(history))
            yield return token;
    }
}
