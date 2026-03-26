namespace NexusAssistant.Api.Config;

public class OllamaConfig
{
    public string Endpoint { get; set; } = "http://localhost:11434";
    public string Model { get; set; } = "deepseek-coder-v2:latest";
    public int TimeOutSeconds { get; set; } = 60000;
}
