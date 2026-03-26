using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using NexusAssistant.Api.Config;

namespace NexusAssistant.Api.Providers;

public class OllamaProvider : ILLMProvider
{
    private readonly OllamaConfig _config;

    public string ProviderName => "Ollama";

    public OllamaProvider(IOptions<NexusConfig> config)
    {
        _config = config.Value.Ollama;
    }

    public Kernel CreateKernel() => CreateKernelForModel(_config.Model);

    public Kernel CreateKernelForModel(string model)
    {
        var httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromMinutes(10),
            BaseAddress = new Uri(_config.Endpoint)
        };

        return Kernel.CreateBuilder()
            .AddOllamaChatCompletion(
                modelId: model,
                httpClient: httpClient)
            .Build();
    }
}