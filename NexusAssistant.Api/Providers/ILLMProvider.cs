using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace NexusAssistant.Api.Providers;

public interface ILLMProvider
{
    string ProviderName { get; }

    Kernel CreateKernel() => CreateKernelForModel(string.Empty);

    Kernel CreateKernelForModel(string model);

    Task<string> CompleteAsync(ChatHistory history)
        => throw new NotImplementedException(
            $"CompleteAsync non implémenté pour ce provider.");
}