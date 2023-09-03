namespace SagaFlow;

/// <summary>
/// Represents a log entry in the saga, containing information about the executed step, the resulting state, and the time of execution.
/// </summary>
/// <typeparam name="TState">The type representing the state of the saga.</typeparam>
public class SagaLogEntry<TState>
{
    /// <summary>
    /// Gets the step associated with this log entry.
    /// </summary>
    public ISagaStep<TState> Step { get; }

    /// <summary>
    /// Gets the state resulting from executing or attempting to execute the step.
    /// </summary>
    public SagaState State { get; }

    /// <summary>
    /// Gets the time when the step was executed or attempted.
    /// </summary>
    public DateTimeOffset Time { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SagaLogEntry{TState}" /> class.
    /// </summary>
    /// <param name="step">The saga step associated with this log entry.</param>
    /// <param name="state">The state resulting from executing or attempting to execute the step.</param>
    public SagaLogEntry(ISagaStep<TState> step, SagaState state)
    {
        Step = step;
        State = state;
        Time = DateTimeOffset.UtcNow;
    }
}
