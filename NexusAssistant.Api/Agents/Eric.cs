using Microsoft.Extensions.Options;
using NexusAssistant.Api.Config;
using NexusAssistant.Api.Models;
using NexusAssistant.Api.Providers;

namespace NexusAssistant.Api.Agents;

public class Eric : Auguste
{
    protected override string SystemPrompt => """
    Tu dois évaluer si une question de développement est TRIVIALE, STANDARD ou COMPLEXE.
    Réponds UNIQUEMENT par TRIVIALE, STANDARD ou COMPLEXE.
    Un seul mot. Aucun autre texte. Aucune ponctuation.
    "C'est quoi X ?" → toujours TRIVIALE
    "Explique X" → toujours TRIVIALE
    
    TRIVIALE si : définition d'un concept, explication d'un mot-clé,
                  question de syntaxe basique, "c'est quoi X ?", 
                  "comment fonctionne X ?", "explique moi X".
                  Exemples : "c'est quoi un delegate ?",
                             "comment déclarer une liste ?",
                             "explique les lambda expressions",
                             "c'est quoi une coroutine Unity ?"
    
    STANDARD si : implémentation d'une fonctionnalité précise,
                  question sur un pattern spécifique,
                  un seul domaine technique impliqué.
                  Exemples : "comment structurer un inventaire Unity ?",
                             "comment faire une API REST basique ?",
                             "implémenter un système de sauvegarde"
    
    COMPLEXE si : architecture complète d'un système,
                  choix entre plusieurs technologies,
                  optimisation de performances,
                  plusieurs domaines techniques impliqués.
                  Exemples : "architecture complète d'une application ?",
                             "comment optimiser les performances d'un jeu ?",
                             "gérer les membres de deux associations"
    """;

    public Eric(ILLMProvider provider, IOptions<NexusConfig> config)
    : base(provider, config, config.Value.Agents.Eric) { }

    public async Task<ComplexityLevel> EvaluateComplexityAsync(string userMessage, string context = "")
    {
        var history = BuildHistory(userMessage, context);
        var response = await CompleteAsync(history);
        return response.Trim().ToUpper() switch
        {
            "TRIVIALE" => ComplexityLevel.Fast,
            "COMPLEXE" => ComplexityLevel.Debate,
            _ => ComplexityLevel.Normal
        };
    }

}
