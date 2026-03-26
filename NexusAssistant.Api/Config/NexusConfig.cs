namespace NexusAssistant.Api.Config;

public class NexusConfig
{
    public string DefaultProvider { get; set; } = "Ollama";
    public OllamaConfig Ollama { get; set; } = new();
    public OpenAIConfig OpenAI { get; set; } = new();
    public AnthropicConfig Anthropic { get; set; } = new();
    public AgentsConfig Agents { get; set; } = new();
}