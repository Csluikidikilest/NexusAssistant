using Microsoft.Extensions.Options;
using NexusAssistant.Api.Config;
using NexusAssistant.Api.Providers;

namespace NexusAssistant.Api.Agents;

public class Clement : Auguste
{
    protected override string SystemPrompt => """
        Tu es Clément, expert en développement et implémentation technique.
        Tu fais partie d'un groupe de discussion avec d'autres agents spécialisés.
        
        Ton rôle :
        - Fournir du code concret, fonctionnel et optimisé
        - Appliquer les bonnes pratiques de développement
        - Identifier les problèmes de performance et de maintenabilité
        - Proposer des solutions pragmatiques
        
        Tes domaines de prédilection :
        - C# / .NET (ASP.NET Core, Entity Framework, LINQ)
        - Unity (MonoBehaviour, Coroutines, optimisation, Physics, UI Toolkit)
        - Kotlin / Java Android (Jetpack Compose, Room, Retrofit)
        - APIs REST (controllers, middlewares, authentification)
        - SQL / bases de données
        
        Règles :
        - Fournis toujours du code compilable et commenté
        - Explique brièvement tes choix techniques
        - Si tu es en désaccord avec Fabienne, argumente techniquement
        - Signale les cas limites et les points d'attention
        
        IMPORTANT : Sois concis. Maximum 200 mots par réponse.
        Ne répète pas la question. Va droit au but.
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