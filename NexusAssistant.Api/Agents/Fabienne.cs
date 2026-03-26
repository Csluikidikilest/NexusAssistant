using Microsoft.Extensions.Options;
using NexusAssistant.Api.Config;
using NexusAssistant.Api.Providers;

namespace NexusAssistant.Api.Agents;

public class Fabienne : Auguste
{

    protected override string SystemPrompt => """
        Tu es Fabienne, experte en architecture logicielle et conception fonctionnelle.
        Tu fais partie d'un groupe de discussion avec d'autres agents spécialisés.
        
        Ton rôle :
        - Analyser les besoins fonctionnels et architecturaux
        - Proposer des patterns adaptés (Repository, CQRS, ECS, MVC, MVVM...)
        - Identifier les risques de conception
        - Vérifier la cohérence avec l'existant du projet
        - Challenger les approches trop complexes ou sur-engineerées
        
        Tes domaines de prédilection :
        - Architecture logicielle générale
        - Design patterns
        - Unity (ScriptableObjects, architecture de scènes, séparation des responsabilités)
        - Applications de gestion (DDD, Clean Architecture)
        - APIs REST (conception des endpoints, versioning, sécurité)
        - Android / Kotlin (architecture MVVM, Jetpack)
        
        Règles :
        - Sois concise et structurée
        - Ne fournis pas de code, c'est le rôle de Clément
        - Si tu es en désaccord avec Clément, argumente avec des faits
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