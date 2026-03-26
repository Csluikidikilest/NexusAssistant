using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using NexusAssistant.Api.Config;
using System.Text;
using System.Text.Json;

namespace NexusAssistant.Api.Providers;

public class AnthropicProvider : ILLMProvider
{
    private readonly AnthropicConfig _config;

    public string ProviderName => "Anthropic";

    public AnthropicProvider(IOptions<NexusConfig> config)
    {
        _config = config.Value.Anthropic;
    }

    public Kernel CreateKernel() => CreateKernelForModel(string.Empty);

    public Kernel CreateKernelForModel(string model) => Kernel.CreateBuilder().Build();

    public async Task<string> CompleteAsync(ChatHistory history)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("x-api-key", _config.ApiKey);
        client.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

        // Convertit le ChatHistory SK en format Anthropic
        var systemPrompt = history
            .FirstOrDefault(m => m.Role == AuthorRole.System)?.Content ?? "";

        var messages = history
            .Where(m => m.Role != AuthorRole.System)
            .Select(m => new
            {
                role = m.Role == AuthorRole.User ? "user" : "assistant",
                content = m.Content
            });

        var body = JsonSerializer.Serialize(new
        {
            model = _config.Model,
            max_tokens = 4096,
            system = systemPrompt,
            messages
        });

        var response = await client.PostAsync(
            "https://api.anthropic.com/v1/messages",
            new StringContent(body, Encoding.UTF8, "application/json"));

        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);

        return doc.RootElement
            .GetProperty("content")[0]
            .GetProperty("text")
            .GetString() ?? string.Empty;
    }
}