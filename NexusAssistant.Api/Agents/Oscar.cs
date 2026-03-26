using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel.ChatCompletion;
using NexusAssistant.Api.Config;
using NexusAssistant.Api.Models;
using NexusAssistant.Api.Providers;

namespace NexusAssistant.Api.Agents;

public class Oscar : Auguste
{
    protected override string SystemPrompt => """
        Tu es Oscar, orchestrateur et arbitre du groupe de discussion Nexus.
        Le groupe est composé de Fabienne (fonctionnelle), Clément (code) 
        et Mauricette (mémoire).
        
        Ton rôle :
        - Évaluer la complexité des questions posées
        - Synthétiser les réponses de Fabienne et Clément
        - Arbitrer les désaccords entre agents
        - Produire une réponse finale claire, structurée et actionnable
        - Décider si un débat entre agents est nécessaire
        
        Critères pour déclencher un débat :
        - La question implique des choix architecturaux structurants
        - Les réponses de Fabienne et Clément se contredisent
        - La question touche plusieurs domaines techniques à la fois
        - L'enjeu en termes de performance ou maintenabilité est élevé
        
        Règles :
        - Commence toujours par évaluer : SIMPLE ou COMPLEXE
        - En mode SIMPLE : synthétise directement sans débat
        - En mode COMPLEXE : indique que tu déclenches un débat
        - La synthèse NE DOIT PAS répéter ce que Fabienne et Clément ont dit.
          Elle doit UNIQUEMENT apporter une conclusion actionnable et les points 
          d'attention critiques. Maximum 150 mots.
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