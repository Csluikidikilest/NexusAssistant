namespace NexusAssistant.Api.Models;

public class PipelineResult
{
    public string FabienneAnalysis { get; init; } = string.Empty;
    public string ClementAnalysis { get; init; } = string.Empty;
    public string? FabienneDebate { get; init; }
    public string? ClementDebate { get; init; }
    public string OscarSynthesis { get; init; } = string.Empty;
    public bool WasDebated { get; init; }
}
