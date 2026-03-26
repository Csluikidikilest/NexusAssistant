using NexusAssistant.Api.Agents;
using NexusAssistant.Api.Models;
using System.Text;
using System.Threading.Channels;

namespace NexusAssistant.Api.Pipeline;

public class AgentPipeline
{
    private readonly Clement _clement;
    private readonly Eric _eric;
    private readonly Fabienne _fabienne;
    private readonly Mauricette _mauricette;
    private readonly Oscar _oscar;

    public AgentPipeline(
        Clement clement,
        Eric eric,
        Fabienne fabienne,
        Mauricette mauricette,
        Oscar oscar)
    {
        _clement = clement;
        _eric = eric;
        _fabienne = fabienne;
        _mauricette = mauricette;
        _oscar = oscar;
    }

    public async IAsyncEnumerable<string> RunStreamAsync(Guid sessionId,string userMessage)
    {
        // Mauricette récupère le contexte
        yield return $"[Mauricette — {Timestamp()}] Recherche dans la mémoire...\n";
        var context = await _mauricette.RememberAsync(sessionId, userMessage);

        if (!string.IsNullOrEmpty(context))
            yield return $"[Mauricette — {Timestamp()}] Contexte trouvé !\n";
        else
            yield return $"[Mauricette — {Timestamp()}] Aucun contexte antérieur.\n";

        // Oscar évalue la complexité en parallèle
        var complexityTask = _eric.EvaluateComplexityAsync(userMessage);

        var complexity = await complexityTask;
        yield return $"[Eric — {Timestamp()}] Mode : {complexity}\n\n";

        string synthesis;

        switch (complexity)
        {
            // ─── MODE FAST : Oscar seul ───────────────────────────────────────
            case ComplexityLevel.Fast:
                {
                    yield return $"[Oscar — {Timestamp()}] Réponse directe :\n";
                    var oscarBuilder = new StringBuilder();
                    await foreach (var token in _oscar.AnswerDirectlyStreamAsync(
                        userMessage, context))
                    {
                        oscarBuilder.Append(token);
                        yield return token;
                    }
                    synthesis = Clean(oscarBuilder.ToString());
                    yield return "\n\n";
                    break;
                }

            // ─── MODE NORMAL : 3 agents ───────────────────────────────────────
            case ComplexityLevel.Normal:
                {
                    var fabienneBuilder = new StringBuilder();
                    var clementBuilder = new StringBuilder();

                    var fabienneTokens = Channel.CreateUnbounded<string>();
                    var clementTokens = Channel.CreateUnbounded<string>();

                    var fabienneTask = Task.Run(async () =>
                    {
                        await foreach (var token in _fabienne.AnalyzeStreamAsync(
                            userMessage, context))
                        {
                            fabienneBuilder.Append(token);
                            await fabienneTokens.Writer.WriteAsync(token);
                        }
                        fabienneTokens.Writer.Complete();
                    });

                    var clementTask = Task.Run(async () =>
                    {
                        await foreach (var token in _clement.ImplementStreamAsync(
                            userMessage, context))
                        {
                            clementBuilder.Append(token);
                            await clementTokens.Writer.WriteAsync(token);
                        }
                        clementTokens.Writer.Complete();
                    });

                    yield return $"[Fabienne — {Timestamp()}] Analyse :\n";
                    await foreach (var token in fabienneTokens.Reader.ReadAllAsync())
                        yield return token;

                    yield return $"\n\n[Clément — {Timestamp()}] Implémentation :\n";
                    await foreach (var token in clementTokens.Reader.ReadAllAsync())
                        yield return token;

                    await Task.WhenAll(fabienneTask, clementTask);

                    var fabienneAnalysis = Clean(fabienneBuilder.ToString());
                    var clementAnalysis = Clean(clementBuilder.ToString());
                    yield return "\n\n";

                    yield return $"[Oscar — {Timestamp()}] Synthèse :\n";
                    var oscarBuilder = new StringBuilder();
                    await foreach (var token in _oscar.SynthesizeStreamAsync(
                        userMessage, fabienneAnalysis, clementAnalysis,
                        context: context))
                    {
                        oscarBuilder.Append(token);
                        yield return token;
                    }
                    synthesis = Clean(oscarBuilder.ToString());
                    yield return "\n\n";
                    break;
                }

            // ─── MODE DÉBAT : 5 agents ────────────────────────────────────────
            default:
                {
                    var fabienneBuilder = new StringBuilder();
                    var clementBuilder = new StringBuilder();

                    var fabienneTokens = Channel.CreateUnbounded<string>();
                    var clementTokens = Channel.CreateUnbounded<string>();

                    var fabienneTask = Task.Run(async () =>
                    {
                        await foreach (var token in _fabienne.AnalyzeStreamAsync(
                            userMessage, context))
                        {
                            fabienneBuilder.Append(token);
                            await fabienneTokens.Writer.WriteAsync(token);
                        }
                        fabienneTokens.Writer.Complete();
                    });

                    var clementTask = Task.Run(async () =>
                    {
                        await foreach (var token in _clement.ImplementStreamAsync(
                            userMessage, context))
                        {
                            clementBuilder.Append(token);
                            await clementTokens.Writer.WriteAsync(token);
                        }
                        clementTokens.Writer.Complete();
                    });

                    yield return $"[Fabienne — {Timestamp()}] Analyse :\n";
                    await foreach (var token in fabienneTokens.Reader.ReadAllAsync())
                        yield return token;

                    yield return $"\n\n[Clément — {Timestamp()}] Implémentation :\n";
                    await foreach (var token in clementTokens.Reader.ReadAllAsync())
                        yield return token;

                    await Task.WhenAll(fabienneTask, clementTask);
                    
                    var fabienneAnalysis = Clean(fabienneBuilder.ToString());
                    var clementAnalysis = Clean(clementBuilder.ToString());
                    yield return "\n\n";

                    // Débat en parallèle
                    yield return $"[Oscar — {Timestamp()}] Débat en cours...\n";

                    var debateTokens = Channel.CreateUnbounded<string>();

                    var debateTask = Task.WhenAll(
                        _fabienne.RespondToDebateAsync(
                            userMessage, clementAnalysis, context),
                        _clement.RespondToDebateAsync(
                            userMessage, fabienneAnalysis, context));

                    var debate = await debateTask;
                    var fabienneDebate = Clean(debate[0]);
                    var clementDebate = Clean(debate[1]);

                    yield return $"[Fabienne — débat — {Timestamp()}]\n{fabienneDebate}\n\n";
                    yield return $"[Clément  — débat — {Timestamp()}]\n{clementDebate}\n\n";

                    yield return $"[Oscar — {Timestamp()}] Synthèse :\n";
                    var oscarBuilder = new StringBuilder();
                    await foreach (var token in _oscar.SynthesizeStreamAsync(
                        userMessage, fabienneAnalysis, clementAnalysis,
                        fabienneDebate, clementDebate, context))
                    {
                        oscarBuilder.Append(token);
                        yield return token;
                    }
                    synthesis = Clean(oscarBuilder.ToString());
                    yield return "\n\n";
                    break;
                }
        }

        // Mauricette mémorise en arrière-plan
        _ = Task.Run(() => _mauricette.LearnAsync(
            sessionId, userMessage, synthesis));

        yield return $"[DONE — {Timestamp()}]\n";
    }

    private static string Clean(string response) => Auguste.Clean(response);

    private static async Task<string> CollectStreamAsync(IAsyncEnumerable<string> stream)
    {
        var builder = new StringBuilder();
        await foreach (var token in stream)
            builder.Append(token);
        return builder.ToString();
    }

    private static string Timestamp() =>
        DateTime.Now.ToString("HH:mm:ss.fff");
}