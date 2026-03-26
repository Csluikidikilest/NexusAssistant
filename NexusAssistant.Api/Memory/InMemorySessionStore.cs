using NexusAssistant.Api.Models;

namespace NexusAssistant.Api.Memory;

public class InMemorySessionStore : ISessionStore
{
    private readonly Dictionary<Guid, NexusSession> _sessions = [];
    private readonly Lock _lock = new();

    public Task<NexusSession> GetOrCreateAsync(Guid sessionId)
    {
        lock (_lock)
        {
            if (!_sessions.TryGetValue(sessionId, out var session))
            {
                session = new NexusSession { Id = sessionId };
                _sessions[sessionId] = session;
            }
            return Task.FromResult(session);
        }
    }

    public Task SaveAsync(NexusSession session)
    {
        lock (_lock)
        {
            _sessions[session.Id] = session;
        }
        return Task.CompletedTask;
    }

    public Task<IEnumerable<NexusSession>> GetAllAsync()
    {
        lock (_lock)
        {
            return Task.FromResult<IEnumerable<NexusSession>>(
                _sessions.Values
                    .OrderByDescending(s => s.LastActivity)
                    .ToList());
        }
    }

    public Task<IEnumerable<NexusSession>> SearchAsync(string query)
    {
        lock (_lock)
        {
            var results = _sessions.Values
                .Where(s => s.Messages.Any(m =>
                    m.Content.Contains(query,
                        StringComparison.OrdinalIgnoreCase)))
                .OrderByDescending(s => s.LastActivity)
                .ToList();

            return Task.FromResult<IEnumerable<NexusSession>>(results);
        }
    }

    public Task DeleteAsync(Guid sessionId)
    {
        lock (_lock)
        {
            _sessions.Remove(sessionId);
        }
        return Task.CompletedTask;
    }
}