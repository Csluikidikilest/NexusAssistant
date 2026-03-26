using NexusAssistant.Api.Models;

namespace NexusAssistant.Api.Memory;

public class PersistentSessionStore : ISessionStore
{
    public Task<NexusSession> GetOrCreateAsync(Guid sessionId)
        => throw new NotImplementedException("Phase 2 — SQLite + Qdrant");

    public Task SaveAsync(NexusSession session)
        => throw new NotImplementedException("Phase 2 — SQLite + Qdrant");

    public Task<IEnumerable<NexusSession>> GetAllAsync()
        => throw new NotImplementedException("Phase 2 — SQLite + Qdrant");

    public Task<IEnumerable<NexusSession>> SearchAsync(string query)
        => throw new NotImplementedException("Phase 2 — SQLite + Qdrant");

    public Task DeleteAsync(Guid sessionId)
        => throw new NotImplementedException("Phase 2 — SQLite + Qdrant");
}