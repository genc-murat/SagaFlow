namespace SagaFlow;

/// <summary>
/// Delegate for handling completed saga steps.
/// </summary>
/// <typeparam name="TState">The type of the saga state.</typeparam>
public delegate void SagaStepCompletedHandler<TState>(ISagaStep<TState> step, TState state);

/// <summary>
/// Delegate for handling failed saga steps.
/// </summary>
/// <typeparam name="TState">The type of the saga state.</typeparam>
public delegate void SagaStepFailedHandler<TState>(ISagaStep<TState> step, TState state, Exception exception);

/// <summary>
/// Interface defining the essential methods and events for a saga step.
/// </summary>
/// <typeparam name="TState">The type of the saga state.</typeparam>
public interface ISagaStep<TState>
{
    /// <summary>
    /// Event triggered when the step is completed successfully.
    /// </summary>
    event SagaStepCompletedHandler<TState> OnStepCompleted;

    /// <summary>
    /// Event triggered when the step fails.
    /// </summary>
    event SagaStepFailedHandler<TState> OnStepFailed;

    /// <summary>
    /// Executes the step logic asynchronously.
    /// </summary>
    /// <param name="state">The current state of the saga.</param>
    /// <param name="cancellationToken">Cancellation token for the task.</param>
    /// <returns>A Task representing the asynchronous execution.</returns>
    Task ExecuteAsync(TState state, CancellationToken cancellationToken);

    /// <summary>
    /// Rolls back the step logic asynchronously.
    /// </summary>
    /// <param name="state">The current state of the saga.</param>
    /// <param name="cancellationToken">Cancellation token for the task.</param>
    /// <returns>A Task representing the asynchronous rollback.</returns>
    Task RollbackAsync(TState state, CancellationToken cancellationToken);

    /// <summary>
    /// Determines whether the step can be executed based on the current state.
    /// </summary>
    /// <param name="currentState">The current state of the saga.</param>
    /// <returns>True if the step can be executed, otherwise false.</returns>
    bool CanExecute(SagaState currentState);

    /// <summary>
    /// Calculates the next state of the saga based on the current state and success flag.
    /// </summary>
    /// <param name="currentState">The current state of the saga.</param>
    /// <param name="success">Whether the current step was successful.</param>
    /// <returns>The next state of the saga.</returns>
    SagaState NextState(SagaState currentState, bool success);

    /// <summary>
    /// Publishes an event based on the current state and saga state.
    /// </summary>
    /// <param name="currentState">The current state of the saga.</param>
    /// <param name="state">The current saga state.</param>
    /// <returns>A Task representing the asynchronous publish operation.</returns>
    Task PublishEventAsync(SagaState currentState, TState state);

    /// <summary>
    /// Gets the dependencies required for this step.
    /// </summary>
    IEnumerable<Type> Dependencies { get; }
}
