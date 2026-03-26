using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using NexusAssistant.Api.Config;

namespace NexusAssistant.Api.Providers;

public class OpenAIProvider : ILLMProvider
{
    private readonly OpenAIConfig _config;

    public string ProviderName => "OpenAI";

    public OpenAIProvider(IOptions<NexusConfig> config)
    {
        _config = config.Value.OpenAI;
    }

    public Kernel CreateKernel() =>
        CreateKernelForModel(_config.Model);

    public Kernel CreateKernelForModel(string model) =>
    Kernel.CreateBuilder()
        .AddOpenAIChatCompletion(
            modelId: model,
            apiKey: _config.ApiKey)
        .Build();
}