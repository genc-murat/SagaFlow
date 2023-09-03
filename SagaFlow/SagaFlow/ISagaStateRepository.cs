namespace SagaFlow;

/// <summary>
/// Defines methods for saving and loading saga state information.
/// </summary>
/// <typeparam name="TState">The type of the saga state.</typeparam>
public interface ISagaStateRepository<TState>
{
    /// <summary>
    /// Saves a new log entry for a given saga asynchronously.
    /// </summary>
    /// <param name="sagaId">The unique identifier of the saga.</param>
    /// <param name="entry">The log entry to be saved.</param>
    /// <returns>A Task representing the asynchronous save operation.</returns>
    Task SaveAsync(Guid sagaId, SagaLogEntry<TState> entry);

    /// <summary>
    /// Loads the log entries for a given saga asynchronously.
    /// </summary>
    /// <param name="sagaId">The unique identifier of the saga.</param>
    /// <returns>A Task that resolves to a list of log entries for the specified saga.</returns>
    Task<List<SagaLogEntry<TState>>> LoadAsync(Guid sagaId);
}
