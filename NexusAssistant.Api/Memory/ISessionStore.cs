using NexusAssistant.Api.Models;

namespace NexusAssistant.Api.Memory;

public interface ISessionStore
{
    Task<NexusSession> GetOrCreateAsync(Guid sessionId);

    Task SaveAsync(NexusSession session);

    Task<IEnumerable<NexusSession>> GetAllAsync();

    Task<IEnumerable<NexusSession>> SearchAsync(string query);

    Task DeleteAsync(Guid sessionId);
}