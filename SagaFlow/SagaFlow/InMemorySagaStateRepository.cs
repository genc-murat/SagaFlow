using System.Collections.Concurrent;

namespace SagaFlow;

/// <summary>
/// An in-memory implementation of ISagaStateRepository for managing the state of sagas.
/// </summary>
/// <typeparam name="TState">The type of the saga state.</typeparam>
public class InMemorySagaStateRepository<TState> : ISagaStateRepository<TState>
{
    /// <summary>
    /// The internal store for holding saga states.
    /// </summary>
    private readonly ConcurrentDictionary<Guid, List<SagaLogEntry<TState>>> _store = new();

    /// <summary>
    /// Saves a new log entry for a given saga.
    /// </summary>
    /// <param name="sagaId">The unique identifier of the saga.</param>
    /// <param name="entry">The log entry to save.</param>
    /// <returns>A Task that represents the asynchronous save operation.</returns>
    public Task SaveAsync(Guid sagaId, SagaLogEntry<TState> entry)
    {
        if (!_store.ContainsKey(sagaId))
        {
            _store[sagaId] = new List<SagaLogEntry<TState>>();
        }

        _store[sagaId].Add(entry);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Loads the log entries for a given saga.
    /// </summary>
    /// <param name="sagaId">The unique identifier of the saga.</param>
    /// <returns>A Task that returns a list of log entries for the saga.</returns>
    public Task<List<SagaLogEntry<TState>>> LoadAsync(Guid sagaId)
    {
        if (_store.ContainsKey(sagaId))
        {
            return Task.FromResult(_store[sagaId]);
        }

        // Return an empty list if the saga doesn't exist.
        return Task.FromResult(new List<SagaLogEntry<TState>>());
    }
}
